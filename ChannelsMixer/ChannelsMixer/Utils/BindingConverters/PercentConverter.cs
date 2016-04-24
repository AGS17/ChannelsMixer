using System;
using System.Globalization;
using System.Windows.Data;

namespace ChannelsMixer.Utils
{
    public class PercentConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                throw new ArgumentException("Must be only 2 values", nameof(values));

            double value1, value2;
            if (!double.TryParse(values[0].ToString(), out value1))
            {
                value1 = 0;
            }
            if (!double.TryParse(values[1].ToString(), out value2))
            {
                return 0.ToString("P");
            }

            return (value1/value2).ToString("P");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

}