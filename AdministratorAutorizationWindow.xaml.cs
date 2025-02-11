using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Vuz_Shedule
{
    public partial class AdministratorAutorizationWindow : Window
    {
        public AdministratorAutorizationWindow()
        {
            InitializeComponent();
        }

        private void EmailTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (EmailTextBox.Text == "Email")
            {
                EmailTextBox.Text = "";
                EmailTextBox.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void EmailTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                EmailTextBox.Text = "Email";
                EmailTextBox.Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160)); // Light gray
            }
        }

        private void PasswordTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordTextBox.Text == "Пароль")
            {
                PasswordTextBox.Text = "";
                PasswordTextBox.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void PasswordTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PasswordTextBox.Text))
            {
                PasswordTextBox.Text = "Пароль";
                PasswordTextBox.Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160)); // Light gray
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AdministratorMainWindow administratorMainWindow = new AdministratorMainWindow();
            administratorMainWindow.Show();
        }
    }
}
