using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using POSSystem.Models;
using POSSystem.Services;
using POSSystem.Views;

namespace POSSystem.UserControls
{
    public partial class InventoryView : UserControl
    {
        private ObservableCollection<Product> _products = new();

        public InventoryView()
        {
            InitializeComponent();
            ProductsGrid.ItemsSource = _products;
            Reload();
        }

        private void Reload()
        {
            _products.Clear();
            foreach (var p in ProductService.GetAll(SearchBox.Text))
                _products.Add(p);

            int low = ProductService.GetLowStock().Count;
            LowStockText.Text = low > 0 ? $"⚠ {low} low-stock items" : "✓ All stock levels OK";
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => Reload();
        private void Refresh_Click(object sender, RoutedEventArgs e) => Reload();

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ProductDialog { Owner = Window.GetWindow(this) };
            if (dlg.ShowDialog() == true) Reload();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Product p)
            {
                var dlg = new ProductDialog(p) { Owner = Window.GetWindow(this) };
                if (dlg.ShowDialog() == true) Reload();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Product p)
            {
                var result = MessageBox.Show($"Delete '{p.Name}'?\n\nThis cannot be undone.", "Confirm Delete",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    ProductService.Delete(p.Id);
                    Reload();
                }
            }
        }
    }
}
