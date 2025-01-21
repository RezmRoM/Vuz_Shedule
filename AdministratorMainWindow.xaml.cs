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
            LoadComboBoxes();
        }

        private void LoadComboBoxes()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Загрузка предметов
                    string subjectsQuery = "SELECT id_predmeta, nazvanie_predmeta FROM RV_Predmet";
                    using (SqlCommand command = new SqlCommand(subjectsQuery, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                SubjectComboBox.Items.Add(new ComboBoxItem 
                                { 
                                    Content = reader["nazvanie_predmeta"].ToString(),
                                    Tag = reader["id_predmeta"]
                                });
                            }
                        }
                    }

                    // Загрузка преподавателей
                    string teachersQuery = "SELECT id_prepodavatelya, CONCAT(familia, ' ', imya, ' ', otchestvo) as fio FROM RV_Prepodavatel";
                    using (SqlCommand command = new SqlCommand(teachersQuery, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TeacherComboBox.Items.Add(new ComboBoxItem 
                                { 
                                    Content = reader["fio"].ToString(),
                                    Tag = reader["id_prepodavatelya"]
                                });
                            }
                        }
                    }

                    // Загрузка аудиторий
                    string classroomsQuery = "SELECT id_auditorii, nazvanie_auditorii FROM RV_Auditoria";
                    using (SqlCommand command = new SqlCommand(classroomsQuery, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ClassroomComboBox.Items.Add(new ComboBoxItem 
                                { 
                                    Content = reader["nazvanie_auditorii"].ToString(),
                                    Tag = reader["id_auditorii"]
                                });
                            }
                        }
                    }

                    // Загрузка дней недели
                    string daysQuery = "SELECT id_den_nedeli, nazvanie FROM RV_Den_Nedeli";
                    using (SqlCommand command = new SqlCommand(daysQuery, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DayComboBox.Items.Add(new ComboBoxItem 
                                { 
                                    Content = reader["nazvanie"].ToString(),
                                    Tag = reader["id_den_nedeli"]
                                });
                            }
                        }
                    }

                    // Загрузка типов занятий
                    string typesQuery = "SELECT id_tip_zanyatia, nazvanie FROM RV_Tip_Zanyatia";
                    using (SqlCommand command = new SqlCommand(typesQuery, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                LessonTypeComboBox.Items.Add(new ComboBoxItem 
                                { 
                                    Content = reader["nazvanie"].ToString(),
                                    Tag = reader["id_tip_zanyatia"]
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput()) return;

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"INSERT INTO RV_Zanyatie 
                                   (id_predmeta, id_prepodavatelya, id_auditorii, id_den_nedeli, 
                                    nomer_pary, chetnost_nedeli, polnost_gruppy, id_tip_zanyatia)
                                   VALUES 
                                   (@SubjectId, @TeacherId, @ClassroomId, @DayId,
                                    @LessonNumber, @WeekParity, @GroupComposition, @LessonTypeId)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SubjectId", (SubjectComboBox.SelectedItem as ComboBoxItem)?.Tag);
                        command.Parameters.AddWithValue("@TeacherId", (TeacherComboBox.SelectedItem as ComboBoxItem)?.Tag);
                        command.Parameters.AddWithValue("@ClassroomId", (ClassroomComboBox.SelectedItem as ComboBoxItem)?.Tag);
                        command.Parameters.AddWithValue("@DayId", (DayComboBox.SelectedItem as ComboBoxItem)?.Tag);
                        command.Parameters.AddWithValue("@LessonNumber", int.Parse(LessonNumberTextBox.Text));
                        command.Parameters.AddWithValue("@WeekParity", WeekParityComboBox.SelectedIndex == 0);
                        command.Parameters.AddWithValue("@GroupComposition", GroupCompositionComboBox.SelectedIndex == 0);
                        command.Parameters.AddWithValue("@LessonTypeId", (LessonTypeComboBox.SelectedItem as ComboBoxItem)?.Tag);

                        command.ExecuteNonQuery();
                        MessageBox.Show("Занятие успешно добавлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        ClearForm();
                        LoadSchedule();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (SubjectComboBox.SelectedItem == null ||
                TeacherComboBox.SelectedItem == null ||
                ClassroomComboBox.SelectedItem == null ||
                DayComboBox.SelectedItem == null ||
                LessonTypeComboBox.SelectedItem == null ||
                WeekParityComboBox.SelectedItem == null ||
                GroupCompositionComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(LessonNumberTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(LessonNumberTextBox.Text, out int lessonNumber) || lessonNumber < 1 || lessonNumber > 8)
            {
                MessageBox.Show("Номер пары должен быть числом от 1 до 8", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            SubjectComboBox.SelectedIndex = -1;
            TeacherComboBox.SelectedIndex = -1;
            ClassroomComboBox.SelectedIndex = -1;
            DayComboBox.SelectedIndex = -1;
            LessonTypeComboBox.SelectedIndex = -1;
            WeekParityComboBox.SelectedIndex = -1;
            GroupCompositionComboBox.SelectedIndex = -1;
            LessonNumberTextBox.Clear();
        }

        private void ScheduleDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selectedRow = ScheduleDataGrid.SelectedItem as DataRowView;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
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
