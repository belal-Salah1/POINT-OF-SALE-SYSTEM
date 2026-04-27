using System.Windows;
using POSSystem.Models;
using POSSystem.Services;

namespace POSSystem.Views
{
    public partial class ProductDialog : Window
    {
        private readonly Product? _editingProduct;

        public ProductDialog()
        {
            InitializeComponent();
            TitleText.Text = "Add Product";
            NameBox.Focus();
        }

        public ProductDialog(Product p) : this()
        {
            _editingProduct = p;
            TitleText.Text = "Edit Product";

            NameBox.Text     = p.Name;
            BarcodeBox.Text  = p.Barcode;
            CostBox.Text     = p.Cost.ToString("0.##");
            PriceBox.Text    = p.Price.ToString("0.##");
            StockBox.Text    = p.Stock.ToString();
            MinStockBox.Text = p.MinStock.ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Visibility = Visibility.Collapsed;

            string name = NameBox.Text.Trim();
            if (string.IsNullOrEmpty(name)) { ShowError("Name is required."); return; }

            if (!decimal.TryParse(PriceBox.Text, out decimal price) || price < 0) { ShowError("Invalid price."); return; }
            if (!decimal.TryParse(CostBox.Text,  out decimal cost)  || cost  < 0) { ShowError("Invalid cost.");  return; }
            if (!int.TryParse(StockBox.Text,    out int stock)      || stock < 0) { ShowError("Invalid stock."); return; }
            if (!int.TryParse(MinStockBox.Text, out int minStock)   || minStock < 0) { ShowError("Invalid min stock."); return; }

            var p = _editingProduct ?? new Product();
            p.Name = name;
            p.Barcode = BarcodeBox.Text.Trim();
            p.Price = price;
            p.Cost = cost;
            p.Stock = stock;
            p.MinStock = minStock;

            if (_editingProduct == null)
                ProductService.Insert(p);
            else
                ProductService.Update(p);

            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ShowError(string msg)
        {
            ErrorText.Text = msg;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
