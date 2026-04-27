using System.Windows;
using System.Windows.Input;
using POSSystem.Services;

namespace POSSystem.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            UsernameBox.Focus();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            DoLogin();
        }

        private void OnEnterKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) DoLogin();
        }

        private void DoLogin()
        {
            string username = UsernameBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter username and password.");
                return;
            }

            var user = AuthService.Login(username, password);
            if (user == null)
            {
                ShowError("Invalid username or password.");
                return;
            }

            SessionManager.CurrentUser = user;

            var main = new MainWindow();
            main.Show();
            this.Close();
        }

        private void SignupLink_Click(object sender, RoutedEventArgs e)
        {
            var signup = new SignupWindow { Owner = this };
            signup.ShowDialog();
        }

        private void ShowError(string msg)
        {
            ErrorText.Text = msg;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
