using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace VAWCV5Tubod.Connection
{
    internal sealed class RememberedLoginStore
    {
        private const string FileName = "remembered-login.dat";
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("VAWCV5Tubod.RememberedLogin.v1");

        private readonly string filePath;

        public RememberedLoginStore()
        {
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "VAWCV5Tubod");

            filePath = Path.Combine(appDataPath, FileName);
        }

        public void Save(int userId, string username)
        {
            if (userId <= 0 || string.IsNullOrWhiteSpace(username))
            {
                Clear();
                return;
            }

            string directoryPath = Path.GetDirectoryName(filePath) ?? string.Empty;

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string payload = string.Join("|", "1", userId.ToString(), username.Trim());
            byte[] protectedBytes = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(payload),
                Entropy,
                DataProtectionScope.CurrentUser);

            File.WriteAllText(filePath, Convert.ToBase64String(protectedBytes));
        }

        public RememberedLogin? Load()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return null;
                }

                string protectedText = File.ReadAllText(filePath).Trim();

                if (string.IsNullOrWhiteSpace(protectedText))
                {
                    Clear();
                    return null;
                }

                byte[] unprotectedBytes = ProtectedData.Unprotect(
                    Convert.FromBase64String(protectedText),
                    Entropy,
                    DataProtectionScope.CurrentUser);

                string payload = Encoding.UTF8.GetString(unprotectedBytes);
                string[] parts = payload.Split(new[] { '|' }, 3);

                if (parts.Length != 3 ||
                    parts[0] != "1" ||
                    !int.TryParse(parts[1], out int userId) ||
                    userId <= 0 ||
                    string.IsNullOrWhiteSpace(parts[2]))
                {
                    Clear();
                    return null;
                }

                return new RememberedLogin(userId, parts[2]);
            }
            catch
            {
                Clear();
                return null;
            }
        }

        public void Clear()
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    internal sealed class RememberedLogin
    {
        public RememberedLogin(int userId, string username)
        {
            UserId = userId;
            Username = username;
        }

        public int UserId { get; }

        public string Username { get; }
    }
}
