using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POSSystem.Models
{
    public class CartItem : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal UnitCost { get; set; }
        public int StockAvailable { get; set; }

        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); OnPropertyChanged(nameof(Subtotal)); }
        }

        public decimal Subtotal => Quantity * UnitPrice;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
