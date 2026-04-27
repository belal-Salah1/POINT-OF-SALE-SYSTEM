using System;
using System.Collections.Generic;
using POSSystem.Data;
using POSSystem.Models;

namespace POSSystem.Services
{
    public static class UserService
    {
        public static List<User> GetAll()
        {
            var list = new List<User>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Username, Role, FullName, CreatedAt FROM Users ORDER BY CreatedAt DESC";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Role = reader.GetString(2),
                    FullName = reader.GetString(3),
                    CreatedAt = DateTime.Parse(reader.GetString(4)),
                });
            }
            return list;
        }

        public static void Delete(int id)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Users WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
