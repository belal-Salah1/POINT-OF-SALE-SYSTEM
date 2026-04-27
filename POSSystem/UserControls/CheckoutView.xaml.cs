using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using POSSystem.Models;
using POSSystem.Services;
using POSSystem.Views;

namespace POSSystem.UserControls
{
    public partial class CheckoutView : UserControl
    {
        private ObservableCollection<Product> _products = new();
        private ObservableCollection<CartItem> _cart = new();

        public CheckoutView()
        {
            InitializeComponent();
            CartGrid.ItemsSource = _cart;
            ProductsGrid.ItemsSource = _products;
            LoadProducts();
            UpdateTotals();
        }

        private void LoadProducts(string? search = null)
        {
            _products.Clear();
            foreach (var p in ProductService.GetAll(search))
                _products.Add(p);
        }

        // ---------- Search ----------
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadProducts(SearchBox.Text);
        }

        // ---------- Add to cart ----------
        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Product p)
                AddProductToCart(p);
        }

        private void ProductsGrid_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ProductsGrid.SelectedItem is Product p)
                AddProductToCart(p);
        }

        private void AddProductToCart(Product p)
        {
            if (p.Stock <= 0)
            {
                MessageBox.Show($"'{p.Name}' is out of stock.", "Out of Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existing = _cart.FirstOrDefault(c => c.ProductId == p.Id);
            if (existing != null)
            {
                if (existing.Quantity + 1 > existing.StockAvailable)
                {
                    MessageBox.Show($"Only {existing.StockAvailable} units available.", "Stock limit", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                existing.Quantity++;
            }
            else
            {
                _cart.Add(new CartItem
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    UnitPrice = p.Price,
                    UnitCost = p.Cost,
                    StockAvailable = p.Stock,
                    Quantity = 1,
                });
            }
            UpdateTotals();
        }

        // ---------- Quantity buttons ----------
        private void Increment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CartItem c)
            {
                if (c.Quantity + 1 > c.StockAvailable)
                {
                    MessageBox.Show($"Only {c.StockAvailable} units available.", "Stock limit", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                c.Quantity++;
                UpdateTotals();
            }
        }

        private void Decrement_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CartItem c)
            {
                if (c.Quantity > 1)
                {
                    c.Quantity--;
                    UpdateTotals();
                }
                else
                {
                    _cart.Remove(c);
                    UpdateTotals();
                }
            }
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CartItem c)
            {
                _cart.Remove(c);
                UpdateTotals();
            }
        }

        private void ClearCart_Click(object sender, RoutedEventArgs e)
        {
            _cart.Clear();
            DiscountBox.Text = "0";
            CashGivenBox.Text = "0";
            UpdateTotals();
        }

        // ---------- Totals ----------
        private void DiscountBox_TextChanged(object sender, TextChangedEventArgs e) => UpdateTotals();
        private void CashGivenBox_TextChanged(object sender, TextChangedEventArgs e) => UpdateChange();

        private void PaymentMethod_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (CashPanel == null) return;
            string method = (PaymentMethodBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Cash";
            CashPanel.Visibility = method == "Cash" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateTotals()
        {
            // Guard: this can fire from DiscountBox.TextChanged during XAML parsing,
            // before later-defined elements (DiscountAmountText, TotalText) exist.
            if (DiscountAmountText == null || TotalText == null || SubtotalText == null) return;

            decimal subtotal = _cart.Sum(c => c.Subtotal);
            decimal.TryParse(DiscountBox.Text, out decimal discountPct);
            if (discountPct < 0) discountPct = 0;
            if (discountPct > 100) discountPct = 100;
            decimal discountAmt = subtotal * (discountPct / 100m);
            decimal total = subtotal - discountAmt;

            SubtotalText.Text = subtotal.ToString("N2");
            DiscountAmountText.Text = "-" + discountAmt.ToString("N2");
            TotalText.Text = total.ToString("N2");
            CartCountText.Text = $"{_cart.Sum(c => c.Quantity)} items";
            UpdateChange();
        }

        private void UpdateChange()
        {
            if (ChangeText == null || TotalText == null || CashGivenBox == null) return;
            decimal.TryParse(CashGivenBox.Text, out decimal cash);
            decimal.TryParse(TotalText.Text, out decimal total);
            decimal change = cash - total;
            ChangeText.Text = change >= 0 ? change.ToString("N2") : "0.00";
        }

        // ---------- Complete sale ----------
        private void CompleteSale_Click(object sender, RoutedEventArgs e)
        {
            if (_cart.Count == 0)
            {
                MessageBox.Show("Cart is empty.", "Nothing to checkout", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string method = (PaymentMethodBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Cash";

            decimal.TryParse(TotalText.Text, out decimal total);

            if (method == "Cash")
            {
                decimal.TryParse(CashGivenBox.Text, out decimal cash);
                if (cash < total)
                {
                    MessageBox.Show($"Cash received ({cash:N2}) is less than the total ({total:N2}).", "Insufficient cash", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            decimal.TryParse(DiscountBox.Text, out decimal discountPct);

            try
            {
                var sale = SaleService.ProcessSale(
                    SessionManager.CurrentUser!.Id,
                    _cart.ToList(),
                    discountPct,
                    method);

                // Show receipt
                var receipt = new ReceiptWindow(sale)
                {
                    Owner = Window.GetWindow(this)
                };
                receipt.ShowDialog();

                // Reset
                _cart.Clear();
                DiscountBox.Text = "0";
                CashGivenBox.Text = "0";
                LoadProducts(SearchBox.Text);
                UpdateTotals();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing sale: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
