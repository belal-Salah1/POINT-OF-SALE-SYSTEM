using System;
using System.Collections.Generic;
using POSSystem.Data;
using POSSystem.Models;

namespace POSSystem.Services
{
    public static class SaleService
    {
        /// <summary>
        /// Records a sale, its items, and decrements product stock — all within one transaction.
        /// Returns the saved Sale (with its new Id).
        /// </summary>
        public static Sale ProcessSale(int userId, List<CartItem> cart, decimal discountPercent, string paymentMethod)
        {
            if (cart == null || cart.Count == 0)
                throw new InvalidOperationException("Cart is empty.");

            decimal subtotal = 0;
            foreach (var item in cart) subtotal += item.Subtotal;

            decimal discountAmount = subtotal * (discountPercent / 100m);
            decimal total = subtotal - discountAmount;

            using var conn = DatabaseHelper.GetConnection();
            using var tx = conn.BeginTransaction();

            try
            {
                // 1. Insert the Sale
                int saleId;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"INSERT INTO Sales (UserId, Subtotal, Discount, DiscountAmount, Total, PaymentMethod, CreatedAt)
                                        VALUES (@u, @s, @d, @da, @t, @p, @c);
                                        SELECT last_insert_rowid();";
                    cmd.Parameters.AddWithValue("@u", userId);
                    cmd.Parameters.AddWithValue("@s", subtotal);
                    cmd.Parameters.AddWithValue("@d", discountPercent);
                    cmd.Parameters.AddWithValue("@da", discountAmount);
                    cmd.Parameters.AddWithValue("@t", total);
                    cmd.Parameters.AddWithValue("@p", paymentMethod);
                    cmd.Parameters.AddWithValue("@c", DateTime.Now.ToString("o"));
                    saleId = (int)(long)(cmd.ExecuteScalar() ?? 0L);
                }

                // 2. Insert each SaleItem and decrement stock
                foreach (var item in cart)
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = @"INSERT INTO SaleItems (SaleId, ProductId, ProductName, Quantity, UnitPrice, UnitCost, Subtotal)
                                            VALUES (@sid, @pid, @pn, @q, @p, @uc, @sub)";
                        cmd.Parameters.AddWithValue("@sid", saleId);
                        cmd.Parameters.AddWithValue("@pid", item.ProductId);
                        cmd.Parameters.AddWithValue("@pn", item.Name);
                        cmd.Parameters.AddWithValue("@q", item.Quantity);
                        cmd.Parameters.AddWithValue("@p", item.UnitPrice);
                        cmd.Parameters.AddWithValue("@uc", item.UnitCost);
                        cmd.Parameters.AddWithValue("@sub", item.Subtotal);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = "UPDATE Products SET Stock = Stock - @q WHERE Id = @id";
                        cmd.Parameters.AddWithValue("@q", item.Quantity);
                        cmd.Parameters.AddWithValue("@id", item.ProductId);
                        cmd.ExecuteNonQuery();
                    }
                }

                tx.Commit();

                return new Sale
                {
                    Id = saleId,
                    UserId = userId,
                    CashierName = SessionManager.CurrentUser?.FullName ?? string.Empty,
                    Subtotal = subtotal,
                    Discount = discountPercent,
                    DiscountAmount = discountAmount,
                    Total = total,
                    PaymentMethod = paymentMethod,
                    CreatedAt = DateTime.Now,
                    Items = ConvertCartToSaleItems(saleId, cart),
                };
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        private static List<SaleItem> ConvertCartToSaleItems(int saleId, List<CartItem> cart)
        {
            var items = new List<SaleItem>();
            foreach (var c in cart)
            {
                items.Add(new SaleItem
                {
                    SaleId = saleId,
                    ProductId = c.ProductId,
                    ProductName = c.Name,
                    Quantity = c.Quantity,
                    UnitPrice = c.UnitPrice,
                    UnitCost = c.UnitCost,
                    Subtotal = c.Subtotal,
                });
            }
            return items;
        }

        public static List<Sale> GetSalesBetween(DateTime from, DateTime to)
        {
            var list = new List<Sale>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT s.Id, s.UserId, u.FullName, s.Subtotal, s.Discount, s.DiscountAmount,
                                       s.Total, s.PaymentMethod, s.CreatedAt
                                FROM Sales s
                                LEFT JOIN Users u ON u.Id = s.UserId
                                WHERE s.CreatedAt >= @from AND s.CreatedAt <= @to
                                ORDER BY s.CreatedAt DESC";
            cmd.Parameters.AddWithValue("@from", from.ToString("o"));
            cmd.Parameters.AddWithValue("@to", to.ToString("o"));

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Sale
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    CashierName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    Subtotal = (decimal)reader.GetDouble(3),
                    Discount = (decimal)reader.GetDouble(4),
                    DiscountAmount = (decimal)reader.GetDouble(5),
                    Total = (decimal)reader.GetDouble(6),
                    PaymentMethod = reader.GetString(7),
                    CreatedAt = DateTime.Parse(reader.GetString(8)),
                });
            }
            return list;
        }
    }
}
