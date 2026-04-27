using System;
using System.IO;
using Microsoft.Data.Sqlite;
using POSSystem.Helpers;

namespace POSSystem.Data
{
    public static class DatabaseHelper
    {
        // pos.db lives next to the .exe so the app stays portable
        public static string DbPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pos.db");

        public static string ConnectionString => $"Data Source={DbPath}";

        public static SqliteConnection GetConnection()
        {
            var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        public static void Initialize()
        {
            using var conn = GetConnection();

            // Create tables
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL UNIQUE,
                        PasswordHash TEXT NOT NULL,
                        Salt TEXT NOT NULL,
                        Role TEXT NOT NULL,
                        FullName TEXT NOT NULL,
                        CreatedAt TEXT NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS Products (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Barcode TEXT,
                        Price REAL NOT NULL,
                        Cost REAL NOT NULL,
                        Stock INTEGER NOT NULL DEFAULT 0,
                        MinStock INTEGER NOT NULL DEFAULT 5
                    );

                    CREATE TABLE IF NOT EXISTS Sales (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER NOT NULL,
                        Subtotal REAL NOT NULL,
                        Discount REAL NOT NULL DEFAULT 0,
                        DiscountAmount REAL NOT NULL DEFAULT 0,
                        Total REAL NOT NULL,
                        PaymentMethod TEXT NOT NULL,
                        CreatedAt TEXT NOT NULL,
                        FOREIGN KEY (UserId) REFERENCES Users(Id)
                    );

                    CREATE TABLE IF NOT EXISTS SaleItems (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        SaleId INTEGER NOT NULL,
                        ProductId INTEGER NOT NULL,
                        ProductName TEXT NOT NULL,
                        Quantity INTEGER NOT NULL,
                        UnitPrice REAL NOT NULL,
                        UnitCost REAL NOT NULL,
                        Subtotal REAL NOT NULL,
                        FOREIGN KEY (SaleId) REFERENCES Sales(Id),
                        FOREIGN KEY (ProductId) REFERENCES Products(Id)
                    );
                ";
                cmd.ExecuteNonQuery();
            }

            SeedDefaultUsers(conn);
            SeedSampleProducts(conn);
        }

        private static void SeedDefaultUsers(SqliteConnection conn)
        {
            // Check if any users already exist
            using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM Users";
            long count = (long)(checkCmd.ExecuteScalar() ?? 0L);
            if (count > 0) return;

            // Seed: 1 admin + 2 cashiers
            CreateUser(conn, "admin",    "admin123",    "Admin",   "System Administrator");
            CreateUser(conn, "cashier1", "cashier123",  "Cashier", "Cashier One");
            CreateUser(conn, "cashier2", "cashier123",  "Cashier", "Cashier Two");
        }

        private static void CreateUser(SqliteConnection conn, string username, string password, string role, string fullName)
        {
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
        }

        private static void SeedSampleProducts(SqliteConnection conn)
        {
            using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM Products";
            long count = (long)(checkCmd.ExecuteScalar() ?? 0L);
            if (count > 0) return;

            var samples = new (string Name, string Barcode, decimal Price, decimal Cost, int Stock, int MinStock)[]
            {
                ("Coca-Cola 330ml",  "5449000000996", 15.00m,  9.00m,  50, 10),
                ("Pepsi 330ml",      "1234567890101", 14.00m,  8.50m,  45, 10),
                ("Lay's Chips 50g",  "1234567890102", 10.00m,  6.00m,  60, 15),
                ("Snickers Bar",     "1234567890103",  8.00m,  4.50m,  30,  8),
                ("Bottled Water 1L", "1234567890104",  5.00m,  2.00m, 100, 20),
                ("Milk 1L",          "1234567890105", 28.00m, 20.00m,  20,  5),
                ("Bread Loaf",       "1234567890106", 12.00m,  7.00m,  15,  5),
                ("Eggs (dozen)",     "1234567890107", 65.00m, 50.00m,  12,  3),
                ("Coffee Pack 200g", "1234567890108", 95.00m, 70.00m,   8,  3),
                ("Sugar 1kg",        "1234567890109", 30.00m, 22.00m,  25,  5),
            };

            foreach (var p in samples)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT INTO Products (Name, Barcode, Price, Cost, Stock, MinStock)
                                    VALUES (@n, @b, @p, @c, @s, @m)";
                cmd.Parameters.AddWithValue("@n", p.Name);
                cmd.Parameters.AddWithValue("@b", p.Barcode);
                cmd.Parameters.AddWithValue("@p", p.Price);
                cmd.Parameters.AddWithValue("@c", p.Cost);
                cmd.Parameters.AddWithValue("@s", p.Stock);
                cmd.Parameters.AddWithValue("@m", p.MinStock);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
