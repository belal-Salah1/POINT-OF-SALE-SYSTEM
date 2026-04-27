using System.Collections.Generic;
using POSSystem.Data;
using POSSystem.Models;

namespace POSSystem.Services
{
    public static class ProductService
    {
        public static List<Product> GetAll(string? search = null)
        {
            var list = new List<Product>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();

            if (string.IsNullOrWhiteSpace(search))
            {
                cmd.CommandText = "SELECT Id, Name, Barcode, Price, Cost, Stock, MinStock FROM Products ORDER BY Name";
            }
            else
            {
                cmd.CommandText = @"SELECT Id, Name, Barcode, Price, Cost, Stock, MinStock
                                    FROM Products
                                    WHERE Name LIKE @s OR Barcode LIKE @s
                                    ORDER BY Name";
                cmd.Parameters.AddWithValue("@s", "%" + search + "%");
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(ReadProduct(reader));
            }
            return list;
        }

        public static Product? GetById(int id)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, Barcode, Price, Cost, Stock, MinStock FROM Products WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? ReadProduct(reader) : null;
        }

        public static List<Product> GetLowStock()
        {
            var list = new List<Product>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, Barcode, Price, Cost, Stock, MinStock FROM Products WHERE Stock <= MinStock ORDER BY Stock";
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(ReadProduct(reader));
            return list;
        }

        public static int Insert(Product p)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Products (Name, Barcode, Price, Cost, Stock, MinStock)
                                VALUES (@n, @b, @p, @c, @s, @m);
                                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@n", p.Name);
            cmd.Parameters.AddWithValue("@b", p.Barcode ?? string.Empty);
            cmd.Parameters.AddWithValue("@p", p.Price);
            cmd.Parameters.AddWithValue("@c", p.Cost);
            cmd.Parameters.AddWithValue("@s", p.Stock);
            cmd.Parameters.AddWithValue("@m", p.MinStock);
            return (int)(long)(cmd.ExecuteScalar() ?? 0L);
        }

        public static void Update(Product p)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Products SET
                                  Name = @n, Barcode = @b, Price = @p, Cost = @c,
                                  Stock = @s, MinStock = @m
                                WHERE Id = @id";
            cmd.Parameters.AddWithValue("@n", p.Name);
            cmd.Parameters.AddWithValue("@b", p.Barcode ?? string.Empty);
            cmd.Parameters.AddWithValue("@p", p.Price);
            cmd.Parameters.AddWithValue("@c", p.Cost);
            cmd.Parameters.AddWithValue("@s", p.Stock);
            cmd.Parameters.AddWithValue("@m", p.MinStock);
            cmd.Parameters.AddWithValue("@id", p.Id);
            cmd.ExecuteNonQuery();
        }

        public static void Delete(int id)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Products WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        private static Product ReadProduct(Microsoft.Data.Sqlite.SqliteDataReader reader)
        {
            return new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Barcode = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Price = (decimal)reader.GetDouble(3),
                Cost = (decimal)reader.GetDouble(4),
                Stock = reader.GetInt32(5),
                MinStock = reader.GetInt32(6),
            };
        }
    }
}
