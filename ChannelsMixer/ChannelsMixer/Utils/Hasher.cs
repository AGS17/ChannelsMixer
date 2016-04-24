using System.Security.Cryptography;
using System.Text;

namespace ChannelsMixer.Utils
{
    public class Hasher
    {
        public static string Md5Hash(string input)
        {
            var md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            foreach (var b in hash)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        public static string DoubleMd5Hash(string input)
        {
            return Md5Hash(Md5Hash(input));
        }
    }
}
