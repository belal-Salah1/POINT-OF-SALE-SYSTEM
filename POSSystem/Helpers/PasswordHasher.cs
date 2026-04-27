using System;
using System.Security.Cryptography;
using System.Text;

namespace POSSystem.Helpers
{
    public static class PasswordHasher
    {
        /// <summary>
        /// Generates a new random 16-byte salt, base64-encoded.
        /// </summary>
        public static string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }

        /// <summary>
        /// Hashes a password using SHA256 over (password + salt).
        /// </summary>
        public static string Hash(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(password + salt);
            byte[] hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Verifies a password against a stored hash + salt.
        /// </summary>
        public static bool Verify(string password, string salt, string expectedHash)
        {
            string actualHash = Hash(password, salt);
            return actualHash == expectedHash;
        }
    }
}
