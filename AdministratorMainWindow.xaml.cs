using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;
using System.Configuration;
using System.Windows.Controls;

namespace Vuz_Shedule
{
    /// <summary>
    /// Логика взаимодействия для AdministratorMainWindow.xaml
    /// </summary>
    public partial class AdministratorMainWindow : Window
    {
        private readonly string _connectionString;
        private DataRowView _selectedRow;

        public AdministratorMainWindow()
        {
            InitializeComponent();
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            LoadSchedule();
            LoadComboBoxes();
        }

        private void LoadComboBoxes()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
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
                                ComboBoxItem item = new ComboBoxItem
                                {
                                    Content = reader["nazvanie_predmeta"].ToString(),
                                    Tag = reader["id_predmeta"]
                                };
                                SubjectComboBox.Items.Add(item);
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
                                ComboBoxItem item = new ComboBoxItem
                                {
                                    Content = reader["fio"].ToString(),
                                    Tag = reader["id_prepodavatelya"]
                                };
                                TeacherComboBox.Items.Add(item);
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
                                ComboBoxItem item = new ComboBoxItem
                                {
                                    Content = reader["nazvanie_auditorii"].ToString(),
                                    Tag = reader["id_auditorii"]
                                };
                                ClassroomComboBox.Items.Add(item);
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
                                ComboBoxItem item = new ComboBoxItem
                                {
                                    Content = reader["nazvanie"].ToString(),
                                    Tag = reader["id_den_nedeli"]
                                };
                                DayComboBox.Items.Add(item);
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
                                ComboBoxItem item = new ComboBoxItem
                                {
                                    Content = reader["nazvanie"].ToString(),
                                    Tag = reader["id_tip_zanyatia"]
                                };
                                LessonTypeComboBox.Items.Add(item);
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
                using (SqlConnection connection = new SqlConnection(_connectionString))
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

                using (SqlConnection connection = new SqlConnection(connectionString))
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

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Получаем ID занятия
                    string getIdQuery = @"
                        SELECT z.id_zanyatia
                        FROM RV_Zanyatie z
                        JOIN RV_Prepodavatel pr ON z.id_prepodavatelya = pr.id_prepodavatelya
                        JOIN RV_Predmet p ON z.id_predmeta = p.id_predmeta
                        JOIN RV_Den_Nedeli dn ON z.id_den_nedeli = dn.id_den_nedeli
                        WHERE CONCAT(pr.familia, ' ', pr.imya, ' ', pr.otchestvo) = @Teacher
                        AND p.nazvanie_predmeta = @Subject
                        AND dn.nazvanie = @Day
                        AND z.nomer_pary = @LessonNumber";

                    using (SqlCommand command = new SqlCommand(getIdQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Teacher", _selectedRow["Преподаватель"].ToString());
                        command.Parameters.AddWithValue("@Subject", _selectedRow["Предмет"].ToString());
                        command.Parameters.AddWithValue("@Day", _selectedRow["День недели"].ToString());
                        command.Parameters.AddWithValue("@LessonNumber", _selectedRow["Номер пары"]);

                        int zanyatieId = (int)command.ExecuteScalar();

                        // Обновляем данные
                        string updateQuery = @"
                            UPDATE RV_Zanyatie 
                            SET id_predmeta = @SubjectId,
                                id_prepodavatelya = @TeacherId,
                                id_auditorii = @ClassroomId,
                                id_den_nedeli = @DayId,
                                nomer_pary = @LessonNumber,
                                chetnost_nedeli = @WeekParity,
                                polnost_gruppy = @GroupComposition,
                                id_tip_zanyatia = @LessonTypeId
                            WHERE id_zanyatia = @ZanyatieId";

                        using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@SubjectId", (SubjectComboBox.SelectedItem as ComboBoxItem)?.Tag);
                            updateCommand.Parameters.AddWithValue("@TeacherId", (TeacherComboBox.SelectedItem as ComboBoxItem)?.Tag);
                            updateCommand.Parameters.AddWithValue("@ClassroomId", (ClassroomComboBox.SelectedItem as ComboBoxItem)?.Tag);
                            updateCommand.Parameters.AddWithValue("@DayId", (DayComboBox.SelectedItem as ComboBoxItem)?.Tag);
                            updateCommand.Parameters.AddWithValue("@LessonNumber", int.Parse(LessonNumberTextBox.Text));
                            updateCommand.Parameters.AddWithValue("@WeekParity", WeekParityComboBox.SelectedIndex == 0);
                            updateCommand.Parameters.AddWithValue("@GroupComposition", GroupCompositionComboBox.SelectedIndex == 0);
                            updateCommand.Parameters.AddWithValue("@LessonTypeId", (LessonTypeComboBox.SelectedItem as ComboBoxItem)?.Tag);
                            updateCommand.Parameters.AddWithValue("@ZanyatieId", zanyatieId);

                            updateCommand.ExecuteNonQuery();
                            MessageBox.Show("Занятие успешно обновлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadSchedule();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                try
                {
                    using (SqlConnection connection = new SqlConnection(_connectionString))
                    {
                        connection.Open();

                        // Получаем ID занятия
                        string getIdQuery = @"
                            SELECT z.id_zanyatia
                            FROM RV_Zanyatie z
                            JOIN RV_Prepodavatel pr ON z.id_prepodavatelya = pr.id_prepodavatelya
                            JOIN RV_Predmet p ON z.id_predmeta = p.id_predmeta
                            JOIN RV_Den_Nedeli dn ON z.id_den_nedeli = dn.id_den_nedeli
                            WHERE CONCAT(pr.familia, ' ', pr.imya, ' ', pr.otchestvo) = @Teacher
                            AND p.nazvanie_predmeta = @Subject
                            AND dn.nazvanie = @Day
                            AND z.nomer_pary = @LessonNumber";

                        using (SqlCommand command = new SqlCommand(getIdQuery, connection))
                        {
                            command.Parameters.AddWithValue("@Teacher", _selectedRow["Преподаватель"].ToString());
                            command.Parameters.AddWithValue("@Subject", _selectedRow["Предмет"].ToString());
                            command.Parameters.AddWithValue("@Day", _selectedRow["День недели"].ToString());
                            command.Parameters.AddWithValue("@LessonNumber", _selectedRow["Номер пары"]);

                            int zanyatieId = (int)command.ExecuteScalar();

                            // Удаляем связи в расписании групп
                            string deleteRaspisanieQuery = "DELETE FROM RV_Raspisanie_Gruppy WHERE id_zanyatia = @ZanyatieId";
                            using (SqlCommand deleteRaspisanieCommand = new SqlCommand(deleteRaspisanieQuery, connection))
                            {
                                deleteRaspisanieCommand.Parameters.AddWithValue("@ZanyatieId", zanyatieId);
                                deleteRaspisanieCommand.ExecuteNonQuery();
                            }

                            // Удаляем само занятие
                            string deleteZanyatieQuery = "DELETE FROM RV_Zanyatie WHERE id_zanyatia = @ZanyatieId";
                            using (SqlCommand deleteZanyatieCommand = new SqlCommand(deleteZanyatieQuery, connection))
                            {
                                deleteZanyatieCommand.Parameters.AddWithValue("@ZanyatieId", zanyatieId);
                                deleteZanyatieCommand.ExecuteNonQuery();
                            }

                            MessageBox.Show("Занятие успешно удалено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadSchedule();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSchedule();
        }
    }
}
