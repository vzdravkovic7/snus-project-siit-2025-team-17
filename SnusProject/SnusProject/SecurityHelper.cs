using System;
using System.Security.Cryptography;
using System.Text;

namespace SnusProject
{
    public static class SecurityHelper
    {
        private static readonly string secretKey = "SecretKeyVerySecret:)";

        public static string ComputeHmac(string message)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                return Convert.ToBase64String(hash);
            }
        }

        public static bool VerifyHmac(string message, string hmacToVerify)
        {
            string computed = ComputeHmac(message);
            return computed == hmacToVerify;
        }
    }
}
