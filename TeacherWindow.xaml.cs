using System;
using System.Data;
using System.Windows;
using System.Data.SqlClient;
using System.Configuration;

namespace Vuz_Shedule
{
    public partial class TeacherWindow : Window
    {
        private readonly string _connectionString;
        private readonly string _teacherEmail;

        public TeacherWindow(string teacherEmail)
        {
            InitializeComponent();
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            _teacherEmail = teacherEmail;
            LoadTeacherSchedule();
        }

        private void LoadTeacherSchedule()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT * FROM RV_Raspisanie_Polnoe
                        WHERE [Преподаватель] = (
                            SELECT CONCAT(familia, ' ', imya, ' ', otchestvo)
                            FROM RV_Prepodavatel
                            WHERE email = @TeacherEmail
                        )
                        ORDER BY 
                            CASE [День недели]
                                WHEN 'Понедельник' THEN 1
                                WHEN 'Вторник' THEN 2
                                WHEN 'Среда' THEN 3
                                WHEN 'Четверг' THEN 4
                                WHEN 'Пятница' THEN 5
                                WHEN 'Суббота' THEN 6
                                WHEN 'Воскресенье' THEN 7
                            END,
                            [Номер пары]";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TeacherEmail", _teacherEmail);
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        TeacherScheduleDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке расписания: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
} 