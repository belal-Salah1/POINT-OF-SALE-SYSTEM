using System.Windows;
using System.Windows.Controls;
using POSSystem.Models;
using POSSystem.Services;
using POSSystem.Views;

namespace POSSystem.UserControls
{
    public partial class UsersView : UserControl
    {
        public UsersView()
        {
            InitializeComponent();
            Reload();
        }

        private void Reload()
        {
            UsersGrid.ItemsSource = UserService.GetAll();
        }

        private void AddCashier_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SignupWindow { Owner = Window.GetWindow(this), AllowAdminRole = false };
            if (dlg.ShowDialog() == true) Reload();
        }

        private void AddAdmin_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SignupWindow { Owner = Window.GetWindow(this), AllowAdminRole = true };
            if (dlg.ShowDialog() == true) Reload();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is User u)
            {
                if (u.Id == SessionManager.CurrentUser?.Id)
                {
                    MessageBox.Show("You cannot delete your own account while logged in.", "Not allowed",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Delete user '{u.Username}'?\n\nThis cannot be undone.", "Confirm Delete",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    UserService.Delete(u.Id);
                    Reload();
                }
            }
        }
    }
}
