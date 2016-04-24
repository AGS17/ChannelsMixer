using System.Text.RegularExpressions;

namespace ChannelsMixer.Utils
{
    public class Validators
    {
        public static bool IsValidName(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && Regex.IsMatch(value, @"\A([a-zA-Zа-яА-Я]+)\Z", RegexOptions.IgnoreCase);
        }

        public static bool IsValidProductName(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && Regex.IsMatch(value, @"\A([a-zA-Zа-яА-Я0-9 \(\)]+)\Z", RegexOptions.IgnoreCase);
        }

        public static bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
        }

        public static bool IsValidDigitalPassword(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && Regex.IsMatch(password, @"\A([0-9.]+)\Z", RegexOptions.IgnoreCase);
        }

        public static bool IsValidPrice(double price)
        {
            if (price < 0)
                return false;

            return true;
        }

        public static bool IsValidBase64String(string base64)
        {
            if (string.IsNullOrWhiteSpace(base64))
                return false;

            return true;
        }

    }
}
