using System;
using System.Windows;
using System.Windows.Controls;
using POSSystem.Services;

namespace POSSystem.UserControls
{
    public partial class ReportsView : UserControl
    {
        public ReportsView()
        {
            InitializeComponent();
            // Default to last 30 days
            FromDate.SelectedDate = DateTime.Today.AddDays(-30);
            ToDate.SelectedDate   = DateTime.Today;
            LoadReport();
        }

        private void Apply_Click(object sender, RoutedEventArgs e) => LoadReport();

        private void Today_Click(object sender, RoutedEventArgs e)
        {
            FromDate.SelectedDate = DateTime.Today;
            ToDate.SelectedDate   = DateTime.Today;
            LoadReport();
        }

        private void Week_Click(object sender, RoutedEventArgs e)
        {
            FromDate.SelectedDate = DateTime.Today.AddDays(-6);
            ToDate.SelectedDate   = DateTime.Today;
            LoadReport();
        }

        private void Month_Click(object sender, RoutedEventArgs e)
        {
            var now = DateTime.Today;
            FromDate.SelectedDate = new DateTime(now.Year, now.Month, 1);
            ToDate.SelectedDate   = now;
            LoadReport();
        }

        private void LoadReport()
        {
            DateTime from = (FromDate.SelectedDate ?? DateTime.Today).Date;
            DateTime to   = (ToDate.SelectedDate   ?? DateTime.Today).Date.AddDays(1).AddTicks(-1); // end-of-day

            var summary = ReportService.GetSummary(from, to);
            SaleCountText.Text  = summary.SaleCount.ToString();
            RevenueText.Text    = summary.Revenue.ToString("N2");
            CostText.Text       = summary.Cost.ToString("N2");
            ProfitText.Text     = summary.Profit.ToString("N2");
            CashTotalText.Text   = summary.CashTotal.ToString("N2");
            CardTotalText.Text   = summary.CardTotal.ToString("N2");
            MobileTotalText.Text = summary.MobileTotal.ToString("N2");

            TopProductsGrid.ItemsSource = ReportService.GetTopProducts(from, to);
            SalesGrid.ItemsSource       = SaleService.GetSalesBetween(from, to);
        }
    }
}
