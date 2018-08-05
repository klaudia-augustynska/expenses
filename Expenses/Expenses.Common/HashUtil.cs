using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.Common
{
    public static class HashUtil
    {
        public static string GenerateSalt()
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            return Convert.ToBase64String(salt);
        }

        public static string Hash(string password, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt.Trim());
            var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, iterations: 10000);
            var bytes = pbkdf2.GetBytes(20);
            return Convert.ToBase64String(bytes);
        }
    }
}
