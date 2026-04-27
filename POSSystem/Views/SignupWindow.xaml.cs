using System.Windows;
using POSSystem.Services;

namespace POSSystem.Views
{
    public partial class SignupWindow : Window
    {
        // If true, this dialog was opened from the admin Users panel and the role can be chosen.
        // Otherwise, registrations are forced to "Cashier" role.
        public bool AllowAdminRole { get; set; } = false;

        public SignupWindow()
        {
            InitializeComponent();
            FullNameBox.Focus();
        }

        private void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            HideMessages();

            string fullName = FullNameBox.Text.Trim();
            string username = UsernameBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirm = ConfirmPasswordBox.Password;

            if (password != confirm)
            {
                ShowError("Passwords do not match.");
                return;
            }

            // Public signup is always Cashier — Admin must be created from the admin panel.
            string role = AllowAdminRole ? "Admin" : "Cashier";

            var (ok, err) = AuthService.Register(username, password, fullName, role);
            if (!ok)
            {
                ShowError(err);
                return;
            }

            ShowSuccess("Account created. You can now log in.");

            // Briefly delay then close so user sees the message.
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = System.TimeSpan.FromMilliseconds(900)
            };
            timer.Tick += (s, ev) => { timer.Stop(); this.DialogResult = true; this.Close(); };
            timer.Start();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ShowError(string msg)
        {
            ErrorText.Text = msg;
            ErrorText.Visibility = Visibility.Visible;
            SuccessText.Visibility = Visibility.Collapsed;
        }

        private void ShowSuccess(string msg)
        {
            SuccessText.Text = msg;
            SuccessText.Visibility = Visibility.Visible;
            ErrorText.Visibility = Visibility.Collapsed;
        }

        private void HideMessages()
        {
            ErrorText.Visibility = Visibility.Collapsed;
            SuccessText.Visibility = Visibility.Collapsed;
        }
    }
}
