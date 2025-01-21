using System;
using System.Data;
using System.Windows;
using System.Data.SqlClient;
using System.Configuration;

namespace Vuz_Shedule
{
    /// <summary>
    /// Логика взаимодействия для AdministratorMainWindow.xaml
    /// </summary>
    public partial class AdministratorMainWindow : Window
    {
        private string connectionString = "data source=stud-mssql.sttec.yar.ru,38325;user id=user122_db;password=user122;MultipleActiveResultSets=True;App=EntityFramework";
        private DataRowView _selectedRow;

        public AdministratorMainWindow()
        {
            InitializeComponent();
            LoadSchedule();
        }

        private void LoadSchedule()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT * FROM RV_Raspisanie_Polnoe
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
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        ScheduleDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке расписания: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ScheduleDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selectedRow = ScheduleDataGrid.SelectedItem as DataRowView;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Реализовать добавление записи
            MessageBox.Show("Функция добавления записи находится в разработке", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRow == null)
            {
                MessageBox.Show("Пожалуйста, выберите запись для редактирования", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: Реализовать редактирование записи
            MessageBox.Show("Функция редактирования записи находится в разработке", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRow == null)
            {
                MessageBox.Show("Пожалуйста, выберите запись для удаления", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить эту запись?", "Подтверждение", 
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // TODO: Реализовать удаление записи
                MessageBox.Show("Функция удаления записи находится в разработке", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSchedule();
        }
    }
}
