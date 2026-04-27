using System;
using System.Collections.Generic;

namespace POSSystem.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CashierName { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; } // percentage (0-100)
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public DateTime CreatedAt { get; set; }
        public List<SaleItem> Items { get; set; } = new();
    }
}
