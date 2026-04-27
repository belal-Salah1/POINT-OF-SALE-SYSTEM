using System.Windows;
using System.Windows.Controls;
using POSSystem.Services;
using POSSystem.UserControls;

namespace POSSystem.Views
{
    public partial class MainWindow : Window
    {
        private Button? _activeNavButton;

        public MainWindow()
        {
            InitializeComponent();

            var user = SessionManager.CurrentUser;
            if (user == null)
            {
                // Failsafe: should never happen but guard anyway
                new LoginWindow().Show();
                this.Close();
                return;
            }

            UserNameText.Text = user.FullName;
            UserRoleText.Text = user.Role;

            // Hide admin-only nav for cashiers
            if (!SessionManager.IsAdmin)
            {
                NavInventory.Visibility = Visibility.Collapsed;
                NavReports.Visibility   = Visibility.Collapsed;
                NavUsers.Visibility     = Visibility.Collapsed;
            }

            // Default view: Checkout
            ShowView("Checkout");
            _activeNavButton = NavCheckout;
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not string tag) return;
            ShowView(tag);
            SetActiveNav(btn);
        }

        private void ShowView(string tag)
        {
            UIElement content = tag switch
            {
                "Checkout"  => new CheckoutView(),
                "Inventory" => new InventoryView(),
                "Reports"   => new ReportsView(),
                "Users"     => new UsersView(),
                _           => new CheckoutView(),
            };

            ContentArea.Content = content;
        }

        private void SetActiveNav(Button btn)
        {
            if (_activeNavButton != null)
                _activeNavButton.Style = (Style)FindResource("NavButton");
            btn.Style = (Style)FindResource("NavButtonActive");
            _activeNavButton = btn;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            SessionManager.Logout();
            new LoginWindow().Show();
            this.Close();
        }
    }
}
