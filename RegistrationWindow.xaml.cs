using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data.SqlClient;
using System.Configuration;
using System.Text.RegularExpressions;

namespace Vuz_Shedule
{
    public partial class RegistrationWindow : Window
    {
        private readonly string _role;
        private string connectionString = "data source=stud-mssql.sttec.yar.ru,38325;user id=user122_db;password=user122;MultipleActiveResultSets=True;App=EntityFramework";

        public RegistrationWindow(string role)
        {
            InitializeComponent();
            _role = role;

            if (_role != "Преподаватель")
            {
                MessageBox.Show("Регистрация доступна только для преподавателей", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                AutorizationWindow authWindow = new AutorizationWindow(_role);
                authWindow.Show();
                this.Close();
                return;
            }

            EmailTextBox.Tag = "Email";
            LastNameTextBox.Tag = "Фамилия";
            FirstNameTextBox.Tag = "Имя";
            MiddleNameTextBox.Tag = "Отчество";
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

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string lastName = LastNameTextBox.Text;
            string firstName = FirstNameTextBox.Text;
            string middleName = MiddleNameTextBox.Text;
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            // Проверка заполнения полей
            if (string.IsNullOrWhiteSpace(email) || email == "Email" ||
                string.IsNullOrWhiteSpace(lastName) || lastName == "Фамилия" ||
                string.IsNullOrWhiteSpace(firstName) || firstName == "Имя" ||
                string.IsNullOrWhiteSpace(middleName) || middleName == "Отчество" ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка формата email
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Пожалуйста, введите корректный email", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка совпадения паролей
            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Проверка существования email
                    string checkEmailQuery = "SELECT COUNT(*) FROM RV_Prepodavatel WHERE email = @Email";
                    using (SqlCommand checkCommand = new SqlCommand(checkEmailQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Email", email);
                        int count = (int)checkCommand.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show("Пользователь с таким email уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    // Регистрация преподавателя
                    string registerQuery = @"INSERT INTO RV_Prepodavatel (email, familia, imya, otchestvo, password)
                                          VALUES (@Email, @LastName, @FirstName, @MiddleName, @Password)";

                    using (SqlCommand command = new SqlCommand(registerQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@LastName", lastName);
                        command.Parameters.AddWithValue("@FirstName", firstName);
                        command.Parameters.AddWithValue("@MiddleName", middleName);
                        command.Parameters.AddWithValue("@Password", password);

                        command.ExecuteNonQuery();

                        MessageBox.Show("Регистрация успешно завершена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        AutorizationWindow authWindow = new AutorizationWindow(_role);
                        authWindow.Show();
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsValidEmail(string email)
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

        private void LoginLink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AutorizationWindow authWindow = new AutorizationWindow(_role);
            authWindow.Show();
            this.Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            AutorizationWindow authWindow = new AutorizationWindow(_role);
            authWindow.Show();
            this.Close();
        }
    }
} 