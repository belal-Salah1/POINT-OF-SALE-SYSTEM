using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POSSystem.Models
{
    public class Product : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Cost { get; set; }

        private int _stock;
        public int Stock
        {
            get => _stock;
            set { _stock = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsLowStock)); }
        }

        public int MinStock { get; set; }

        public bool IsLowStock => Stock <= MinStock;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
