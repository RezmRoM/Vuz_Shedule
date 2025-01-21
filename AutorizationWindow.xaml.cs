using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data.SqlClient;
using System.Configuration;

namespace Vuz_Shedule
{
    public partial class AutorizationWindow : Window
    {
        private readonly string _role;
        private readonly string _connectionString;

        public AutorizationWindow(string role)
        {
            InitializeComponent();
            _role = role;
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            if (_role == "Администратор")
            {
                RegisterLink.Visibility = Visibility.Collapsed;
            }

            EmailTextBox.Tag = "Email";
            PasswordBox.Tag = "Пароль";
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == textBox.Tag.ToString())
            {
                textBox.Text = string.Empty;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = textBox.Tag.ToString();
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || email == "Email" ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "";

                    if (_role == "Администратор")
                    {
                        query = "SELECT COUNT(*) FROM RV_Administrator WHERE email = @Email AND parol = @Password";
                    }
                    // Добавьте здесь проверку для преподавателей, когда создадите соответствующую таблицу

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", password);

                        int count = (int)command.ExecuteScalar();

                        if (count > 0)
                        {
                            if (_role == "Администратор")
                            {
                                AdministratorMainWindow adminWindow = new AdministratorMainWindow();
                                adminWindow.Show();
                                this.Close();
                            }
                            // Добавьте здесь переход на окно преподавателя
                        }
                        else
                        {
                            MessageBox.Show("Неверный email или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении к базе данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterLink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RegistrationWindow registrationWindow = new RegistrationWindow(_role);
            registrationWindow.Show();
            this.Close();
        }
    }
} 