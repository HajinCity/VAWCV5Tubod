using System;
using System.Security.Cryptography;
using System.Text;

namespace VAWCV5Tubod.Connection
{
    internal static class PasswordSecurity
    {
        public static string HashPassword(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes;

            using (SHA256 sha256 = SHA256.Create())
            {
                hashBytes = sha256.ComputeHash(passwordBytes);
            }

            return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        }

        public static bool Matches(string inputPassword, string? storedPassword)
        {
            if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedPassword))
            {
                return false;
            }

            string hashedInput = HashPassword(inputPassword);

            return string.Equals(storedPassword, hashedInput, StringComparison.OrdinalIgnoreCase)
                || string.Equals(storedPassword, inputPassword, StringComparison.Ordinal);
        }
    }
}
