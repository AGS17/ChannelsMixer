using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ChannelsMixer.Utils
{
    public class MathConverter :
        MarkupExtension,
        IMultiValueConverter,
        IValueConverter
    {
        private readonly Dictionary<string, IExpression> storedExpressions = new Dictionary<string, IExpression>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return this.Convert(new[] {value}, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var result = this.Parse(parameter.ToString()).Eval(values);
                if (targetType == typeof (decimal)) return result;
                if (targetType == typeof (string)) return result.ToString();
                if (targetType == typeof (int)) return (int) result;
                if (targetType == typeof (double)) return (double) result;
                if (targetType == typeof (long)) return (long) result;
                throw new ArgumentException($"Unsupported target type {targetType.FullName}");
            }
            catch (Exception ex)
            {
                this.ProcessException(ex);
            }

            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        protected virtual void ProcessException(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        private IExpression Parse(string s)
        {
            IExpression result;
            if (!this.storedExpressions.TryGetValue(s, out result))
            {
                result = new Parser().Parse(s);
                this.storedExpressions[s] = result;
            }

            return result;
        }

        private interface IExpression
        {
            decimal Eval(object[] args);
        }

        private class Constant : IExpression
        {
            private decimal _value;

            public Constant(string text)
            {
                if (!decimal.TryParse(text, out this._value))
                {
                    throw new ArgumentException($"'{text}' is not a valid number");
                }
            }

            public decimal Eval(object[] args)
            {
                return this._value;
            }
        }

        private class Variable : IExpression
        {
            private int _index;

            public Variable(string text)
            {
                if (!int.TryParse(text, out this._index) || this._index < 0)
                {
                    throw new ArgumentException($"'{text}' is not a valid parameter index");
                }
            }

            public Variable(int n)
            {
                this._index = n;
            }

            public decimal Eval(object[] args)
            {
                if (this._index >= args.Length)
                {
                    throw new ArgumentException(
                        $"MathConverter: parameter index {this._index} is out of range. {args.Length} parameter(s) supplied");
                }

                return System.Convert.ToDecimal(args[this._index]);
            }
        }

        private class BinaryOperation : IExpression
        {
            private readonly Func<decimal, decimal, decimal> operation;
            private readonly IExpression left;
            private readonly IExpression right;

            public BinaryOperation(char operation, IExpression left, IExpression right)
            {
                this.left = left;
                this.right = right;
                switch (operation)
                {
                    case '+':
                        this.operation = (a, b) => (a + b);
                        break;
                    case '-':
                        this.operation = (a, b) => (a - b);
                        break;
                    case '*':
                        this.operation = (a, b) => (a*b);
                        break;
                    case '/':
                        this.operation = (a, b) => (a/b);
                        break;
                    case '%':
                        this.operation = (a, b) => (a%b);
                        break;
                    default:
                        throw new ArgumentException("Invalid operation " + operation);
                }
            }

            public decimal Eval(object[] args)
            {
                return this.operation(this.left.Eval(args), this.right.Eval(args));
            }
        }

        private class Negate : IExpression
        {
            private readonly IExpression param;

            public Negate(IExpression param)
            {
                this.param = param;
            }

            public decimal Eval(object[] args)
            {
                return -this.param.Eval(args);
            }
        }

        private class Parser
        {
            private string text;
            private int pos;

            public IExpression Parse(string text)
            {
                try
                {
                    this.pos = 0;
                    this.text = text;
                    var result = this.ParseExpression();
                    this.RequireEndOfText();
                    return result;
                }
                catch (Exception ex)
                {
                    string msg =
                        $"MathConverter: error parsing expression '{text}'. {ex.Message} at position {this.pos}";

                    throw new ArgumentException(msg, ex);
                }
            }

            private IExpression ParseExpression()
            {
                IExpression left = this.ParseTerm();

                while (true)
                {
                    if (this.pos >= this.text.Length) return left;

                    var c = this.text[this.pos];

                    if (c == '+' || c == '-')
                    {
                        ++this.pos;
                        var right = this.ParseTerm();
                        left = new BinaryOperation(c, left, right);
                    }
                    else
                    {
                        return left;
                    }
                }
            }

            private IExpression ParseTerm()
            {
                IExpression left = this.ParseFactor();

                while (true)
                {
                    if (this.pos >= this.text.Length) return left;

                    var c = this.text[this.pos];

                    if (c == '*' || c == '/')
                    {
                        ++this.pos;
                        IExpression right = this.ParseFactor();
                        left = new BinaryOperation(c, left, right);
                    }
                    else
                    {
                        return left;
                    }
                }
            }

            private IExpression ParseFactor()
            {
                this.SkipWhiteSpace();
                if (this.pos >= this.text.Length) throw new ArgumentException("Unexpected end of text");

                var c = this.text[this.pos];

                if (c == '+')
                {
                    ++this.pos;
                    return this.ParseFactor();
                }

                if (c == '-')
                {
                    ++this.pos;
                    return new Negate(this.ParseFactor());
                }

                if (c == 'x' || c == 'a') return this.CreateVariable(0);
                if (c == 'y' || c == 'b') return this.CreateVariable(1);
                if (c == 'z' || c == 'c') return this.CreateVariable(2);
                if (c == 't' || c == 'd') return this.CreateVariable(3);

                if (c == '(')
                {
                    ++this.pos;
                    var expression = this.ParseExpression();
                    this.SkipWhiteSpace();
                    this.Require(')');
                    this.SkipWhiteSpace();
                    return expression;
                }

                if (c == '{')
                {
                    ++this.pos;
                    var end = this.text.IndexOf('}', this.pos);
                    if (end < 0)
                    {
                        --this.pos;
                        throw new ArgumentException("Unmatched '{'");
                    }
                    if (end == this.pos)
                    {
                        throw new ArgumentException("Missing parameter index after '{'");
                    }
                    var result = new Variable(this.text.Substring(this.pos, end - this.pos).Trim());
                    this.pos = end + 1;
                    this.SkipWhiteSpace();
                    return result;
                }

                const string decimalRegEx = @"(\d+\.?\d*|\d*\.?\d+)";
                var match = Regex.Match(this.text.Substring(this.pos), decimalRegEx);
                if (match.Success)
                {
                    this.pos += match.Length;
                    this.SkipWhiteSpace();
                    return new Constant(match.Value);
                }
                else
                {
                    throw new ArgumentException($"Unexpeted character '{c}'");
                }
            }

            private IExpression CreateVariable(int n)
            {
                ++this.pos;
                this.SkipWhiteSpace();
                return new Variable(n);
            }

            private void SkipWhiteSpace()
            {
                while (this.pos < this.text.Length && char.IsWhiteSpace((this.text[this.pos]))) ++this.pos;
            }

            private void Require(char c)
            {
                if (this.pos >= this.text.Length || this.text[this.pos] != c)
                {
                    throw new ArgumentException("Expected '" + c + "'");
                }

                ++this.pos;
            }

            private void RequireEndOfText()
            {
                if (this.pos != this.text.Length)
                {
                    throw new ArgumentException("Unexpected character '" + this.text[this.pos] + "'");
                }
            }
        }
    }
}