using System.Windows;
using System.Windows.Controls;
using POSSystem.Models;

namespace POSSystem.Views
{
    public partial class ReceiptWindow : Window
    {
        private readonly Sale _sale;

        public ReceiptWindow(Sale sale)
        {
            InitializeComponent();
            _sale = sale;
            FillReceipt();
        }

        private void FillReceipt()
        {
            ReceiptNoText.Text = "#" + _sale.Id.ToString("D6");
            DateText.Text      = _sale.CreatedAt.ToString("yyyy-MM-dd HH:mm");
            CashierText.Text   = _sale.CashierName;
            PaymentText.Text   = _sale.PaymentMethod;
            SubtotalText.Text  = _sale.Subtotal.ToString("N2");

            if (_sale.Discount > 0)
            {
                DiscountLabel.Text = $"Discount ({_sale.Discount:N0}%):";
                DiscountText.Text  = "-" + _sale.DiscountAmount.ToString("N2");
            }
            else
            {
                DiscountLabel.Visibility = Visibility.Collapsed;
                DiscountText.Visibility  = Visibility.Collapsed;
            }

            TotalText.Text     = _sale.Total.ToString("N2");
            ItemsList.ItemsSource = _sale.Items;
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Controls.PrintDialog();
            if (dlg.ShowDialog() == true)
            {
                dlg.PrintVisual(ReceiptContent, "Sales Receipt #" + _sale.Id);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
