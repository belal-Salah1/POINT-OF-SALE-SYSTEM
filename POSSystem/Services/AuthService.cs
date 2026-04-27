using System;
using POSSystem.Data;
using POSSystem.Helpers;
using POSSystem.Models;

namespace POSSystem.Services
{
    public static class AuthService
    {
        /// <summary>
        /// Attempts to authenticate. Returns the User on success, null on failure.
        /// </summary>
        public static User? Login(string username, string password)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Username, PasswordHash, Salt, Role, FullName, CreatedAt FROM Users WHERE Username = @u";
            cmd.Parameters.AddWithValue("@u", username);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            var user = new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Salt = reader.GetString(3),
                Role = reader.GetString(4),
                FullName = reader.GetString(5),
                CreatedAt = DateTime.Parse(reader.GetString(6))
            };

            return PasswordHasher.Verify(password, user.Salt, user.PasswordHash) ? user : null;
        }

        /// <summary>
        /// Registers a new user. Returns (success, errorMessage).
        /// </summary>
        public static (bool Success, string Error) Register(string username, string password, string fullName, string role)
        {
            if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
                return (false, "Username must be at least 3 characters.");
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                return (false, "Password must be at least 6 characters.");
            if (string.IsNullOrWhiteSpace(fullName))
                return (false, "Full name is required.");
            if (role != "Admin" && role != "Cashier")
                return (false, "Role must be 'Admin' or 'Cashier'.");

            using var conn = DatabaseHelper.GetConnection();

            // Username uniqueness check
            using (var check = conn.CreateCommand())
            {
                check.CommandText = "SELECT COUNT(*) FROM Users WHERE Username = @u";
                check.Parameters.AddWithValue("@u", username);
                long exists = (long)(check.ExecuteScalar() ?? 0L);
                if (exists > 0) return (false, "Username already exists.");
            }

            string salt = PasswordHasher.GenerateSalt();
            string hash = PasswordHasher.Hash(password, salt);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Users (Username, PasswordHash, Salt, Role, FullName, CreatedAt)
                                VALUES (@u, @h, @s, @r, @f, @c)";
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@h", hash);
            cmd.Parameters.AddWithValue("@s", salt);
            cmd.Parameters.AddWithValue("@r", role);
            cmd.Parameters.AddWithValue("@f", fullName);
            cmd.Parameters.AddWithValue("@c", DateTime.Now.ToString("o"));
            cmd.ExecuteNonQuery();

            return (true, string.Empty);
        }
    }
}
