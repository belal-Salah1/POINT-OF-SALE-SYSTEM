using System;
using System.Collections.Generic;
using POSSystem.Data;

namespace POSSystem.Services
{
    public static class ReportService
    {
        public class ReportSummary
        {
            public int SaleCount { get; set; }
            public decimal Revenue { get; set; }
            public decimal Cost { get; set; }
            public decimal Profit => Revenue - Cost;
            public decimal CashTotal { get; set; }
            public decimal CardTotal { get; set; }
            public decimal MobileTotal { get; set; }
        }

        public class TopProduct
        {
            public string ProductName { get; set; } = string.Empty;
            public int QuantitySold { get; set; }
            public decimal Revenue { get; set; }
        }

        public static ReportSummary GetSummary(DateTime from, DateTime to)
        {
            var s = new ReportSummary();
            using var conn = DatabaseHelper.GetConnection();

            // Sales count + revenue
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT COUNT(*), COALESCE(SUM(Total), 0)
                                    FROM Sales
                                    WHERE CreatedAt >= @from AND CreatedAt <= @to";
                cmd.Parameters.AddWithValue("@from", from.ToString("o"));
                cmd.Parameters.AddWithValue("@to", to.ToString("o"));
                using var r = cmd.ExecuteReader();
                if (r.Read())
                {
                    s.SaleCount = r.GetInt32(0);
                    s.Revenue = (decimal)r.GetDouble(1);
                }
            }

            // Cost (from sale items, sum of UnitCost * Quantity)
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT COALESCE(SUM(si.UnitCost * si.Quantity), 0)
                                    FROM SaleItems si
                                    JOIN Sales s ON s.Id = si.SaleId
                                    WHERE s.CreatedAt >= @from AND s.CreatedAt <= @to";
                cmd.Parameters.AddWithValue("@from", from.ToString("o"));
                cmd.Parameters.AddWithValue("@to", to.ToString("o"));
                s.Cost = (decimal)(double)(cmd.ExecuteScalar() ?? 0.0);
            }

            // Totals by payment method
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT PaymentMethod, COALESCE(SUM(Total), 0)
                                    FROM Sales
                                    WHERE CreatedAt >= @from AND CreatedAt <= @to
                                    GROUP BY PaymentMethod";
                cmd.Parameters.AddWithValue("@from", from.ToString("o"));
                cmd.Parameters.AddWithValue("@to", to.ToString("o"));
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    string method = r.GetString(0);
                    decimal total = (decimal)r.GetDouble(1);
                    if (method == "Cash") s.CashTotal = total;
                    else if (method == "Card") s.CardTotal = total;
                    else if (method == "Mobile") s.MobileTotal = total;
                }
            }

            return s;
        }

        public static List<TopProduct> GetTopProducts(DateTime from, DateTime to, int limit = 5)
        {
            var list = new List<TopProduct>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT si.ProductName, SUM(si.Quantity) AS QSold, SUM(si.Subtotal) AS Rev
                                FROM SaleItems si
                                JOIN Sales s ON s.Id = si.SaleId
                                WHERE s.CreatedAt >= @from AND s.CreatedAt <= @to
                                GROUP BY si.ProductName
                                ORDER BY QSold DESC
                                LIMIT @lim";
            cmd.Parameters.AddWithValue("@from", from.ToString("o"));
            cmd.Parameters.AddWithValue("@to", to.ToString("o"));
            cmd.Parameters.AddWithValue("@lim", limit);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new TopProduct
                {
                    ProductName = reader.GetString(0),
                    QuantitySold = reader.GetInt32(1),
                    Revenue = (decimal)reader.GetDouble(2),
                });
            }
            return list;
        }
    }
}
