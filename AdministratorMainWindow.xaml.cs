using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;
using System.Configuration;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Windows.Data;

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
            LoadFaculties();
            LoadGroups();
            LoadTeachers();
            LoadSubjects();
            LoadClassrooms();
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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput()) return;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Добавляем занятие
                            string insertZanyatieQuery = @"
                                INSERT INTO RV_Zanyatie 
                                (id_predmeta, id_prepodavatelya, id_auditorii, id_den_nedeli, 
                                 nomer_pary, chetnost_nedeli, polnost_gruppy, id_tip_zanyatia)
                                OUTPUT INSERTED.id_zanyatia
                                VALUES 
                                (@SubjectId, @TeacherId, @ClassroomId, @DayId,
                                 @LessonNumber, @WeekParity, @GroupComposition, @LessonTypeId)";

                            int zanyatieId;
                            using (SqlCommand command = new SqlCommand(insertZanyatieQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@SubjectId", (SubjectComboBox.SelectedItem as ComboBoxItem)?.Tag);
                                command.Parameters.AddWithValue("@TeacherId", (TeacherComboBox.SelectedItem as ComboBoxItem)?.Tag);
                                command.Parameters.AddWithValue("@ClassroomId", (ClassroomComboBox.SelectedItem as ComboBoxItem)?.Tag);
                                command.Parameters.AddWithValue("@DayId", (DayComboBox.SelectedItem as ComboBoxItem)?.Tag);
                                command.Parameters.AddWithValue("@LessonNumber", int.Parse(LessonNumberTextBox.Text));
                                command.Parameters.AddWithValue("@WeekParity", WeekParityComboBox.SelectedIndex == 0);
                                command.Parameters.AddWithValue("@GroupComposition", GroupCompositionComboBox.SelectedIndex == 0);
                                command.Parameters.AddWithValue("@LessonTypeId", (LessonTypeComboBox.SelectedItem as ComboBoxItem)?.Tag);

                                zanyatieId = (int)command.ExecuteScalar();
                            }

                            // Получаем список групп с соответствующими параметрами
                            string selectGroupsQuery = @"
                                SELECT id_gruppy 
                                FROM RV_Gruppa 
                                WHERE chetnost_nedeli = @WeekParity 
                                AND polnost_gruppy = @GroupComposition";

                            List<int> groupIds = new List<int>();
                            using (SqlCommand command = new SqlCommand(selectGroupsQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@WeekParity", WeekParityComboBox.SelectedIndex == 0);
                                command.Parameters.AddWithValue("@GroupComposition", GroupCompositionComboBox.SelectedIndex == 0);

                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        groupIds.Add(reader.GetInt32(0));
                                    }
                                }
                            }

                            // Добавляем связи с группами
                            string insertRaspisanieQuery = @"
                                INSERT INTO RV_Raspisanie_Gruppy (id_gruppy, id_zanyatia)
                                VALUES (@GroupId, @ZanyatieId)";

                            foreach (int groupId in groupIds)
                            {
                                using (SqlCommand command = new SqlCommand(insertRaspisanieQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@GroupId", groupId);
                                    command.Parameters.AddWithValue("@ZanyatieId", zanyatieId);
                                    command.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            MessageBox.Show("Занятие успешно добавлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            ClearForm();
                            LoadSchedule();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception($"Ошибка при добавлении данных: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Добавляем занятие
                            string insertZanyatieQuery = @"
                                INSERT INTO RV_Zanyatie 
                                (id_predmeta, id_prepodavatelya, id_auditorii, id_den_nedeli, 
                                 nomer_pary, chetnost_nedeli, polnost_gruppy, id_tip_zanyatia)
                                OUTPUT INSERTED.id_zanyatia
                                VALUES 
                                (@SubjectId, @TeacherId, @ClassroomId, @DayId,
                                 @LessonNumber, @WeekParity, @GroupComposition, @LessonTypeId)";

                            int zanyatieId;
                            using (SqlCommand command = new SqlCommand(insertZanyatieQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@SubjectId", (SubjectComboBox.SelectedItem as ComboBoxItem)?.Tag);
                                command.Parameters.AddWithValue("@TeacherId", (TeacherComboBox.SelectedItem as ComboBoxItem)?.Tag);
                                command.Parameters.AddWithValue("@ClassroomId", (ClassroomComboBox.SelectedItem as ComboBoxItem)?.Tag);
                                command.Parameters.AddWithValue("@DayId", (DayComboBox.SelectedItem as ComboBoxItem)?.Tag);
                                command.Parameters.AddWithValue("@LessonNumber", int.Parse(LessonNumberTextBox.Text));
                                command.Parameters.AddWithValue("@WeekParity", WeekParityComboBox.SelectedIndex == 0);
                                command.Parameters.AddWithValue("@GroupComposition", GroupCompositionComboBox.SelectedIndex == 0);
                                command.Parameters.AddWithValue("@LessonTypeId", (LessonTypeComboBox.SelectedItem as ComboBoxItem)?.Tag);

                                zanyatieId = (int)command.ExecuteScalar();
                            }

                            // Получаем список групп с соответствующими параметрами
                            string selectGroupsQuery = @"
                                SELECT id_gruppy 
                                FROM RV_Gruppa 
                                WHERE chetnost_nedeli = @WeekParity 
                                AND polnost_gruppy = @GroupComposition";

                            List<int> groupIds = new List<int>();
                            using (SqlCommand command = new SqlCommand(selectGroupsQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@WeekParity", WeekParityComboBox.SelectedIndex == 0);
                                command.Parameters.AddWithValue("@GroupComposition", GroupCompositionComboBox.SelectedIndex == 0);

                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        groupIds.Add(reader.GetInt32(0));
                                    }
                                }
                            }

                            // Добавляем связи с группами
                            string insertRaspisanieQuery = @"
                                INSERT INTO RV_Raspisanie_Gruppy (id_gruppy, id_zanyatia)
                                VALUES (@GroupId, @ZanyatieId)";

                            foreach (int groupId in groupIds)
                            {
                                using (SqlCommand command = new SqlCommand(insertRaspisanieQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@GroupId", groupId);
                                    command.Parameters.AddWithValue("@ZanyatieId", zanyatieId);
                                    command.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            MessageBox.Show("Занятие успешно сохранено! Нажмите кнопку 'Обновить' для отображения изменений.",
                                          "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            ClearForm();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception($"Ошибка при сохранении данных: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (_selectedRow != null)
            {
                // Заполняем поля формы данными из выбранной строки
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        // Заполняем ComboBox'ы соответствующими значениями
                        foreach (ComboBoxItem item in SubjectComboBox.Items)
                        {
                            if (item.Content.ToString() == _selectedRow["Предмет"].ToString())
                            {
                                SubjectComboBox.SelectedItem = item;
                                break;
                            }
                        }

                        foreach (ComboBoxItem item in TeacherComboBox.Items)
                        {
                            if (item.Content.ToString() == _selectedRow["Преподаватель"].ToString())
                            {
                                TeacherComboBox.SelectedItem = item;
                                break;
                            }
                        }

                        foreach (ComboBoxItem item in ClassroomComboBox.Items)
                        {
                            if (item.Content.ToString() == _selectedRow["Аудитория"].ToString())
                            {
                                ClassroomComboBox.SelectedItem = item;
                                break;
                            }
                        }

                        foreach (ComboBoxItem item in DayComboBox.Items)
                        {
                            if (item.Content.ToString() == _selectedRow["День недели"].ToString())
                            {
                                DayComboBox.SelectedItem = item;
                                break;
                            }
                        }

                        foreach (ComboBoxItem item in LessonTypeComboBox.Items)
                        {
                            if (item.Content.ToString() == _selectedRow["Тип занятия"].ToString())
                            {
                                LessonTypeComboBox.SelectedItem = item;
                                break;
                            }
                        }

                        // Заполняем номер пары
                        LessonNumberTextBox.Text = _selectedRow["Номер пары"].ToString();

                        // Заполняем четность недели
                        WeekParityComboBox.SelectedIndex = _selectedRow["Четность недели"].ToString() == "Числитель" ? 0 : 1;

                        // Заполняем состав группы
                        GroupCompositionComboBox.SelectedIndex = _selectedRow["Состав группы"].ToString() == "Полная группа" ? 0 : 1;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при заполнении формы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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
                if (!ValidateInput()) return;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            int zanyatieId = Convert.ToInt32(_selectedRow["ID"]);

                            // Удаляем старые связи с группами
                            string deleteRaspisanieQuery = "DELETE FROM RV_Raspisanie_Gruppy WHERE id_zanyatia = @ZanyatieId";
                            using (SqlCommand command = new SqlCommand(deleteRaspisanieQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@ZanyatieId", zanyatieId);
                                command.ExecuteNonQuery();
                            }

                            // Обновляем данные занятия
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

                            using (SqlCommand command = new SqlCommand(updateQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@SubjectId", (SubjectComboBox.SelectedItem as ComboBoxItem)?.Tag);
                                command.Parameters.AddWithValue("@TeacherId", (TeacherComboBox.SelectedItem as ComboBoxItem)?.Tag);
                                command.Parameters.AddWithValue("@ClassroomId", (ClassroomComboBox.SelectedItem as ComboBoxItem)?.Tag);
                                command.Parameters.AddWithValue("@DayId", (DayComboBox.SelectedItem as ComboBoxItem)?.Tag);
                                command.Parameters.AddWithValue("@LessonNumber", int.Parse(LessonNumberTextBox.Text));
                                command.Parameters.AddWithValue("@WeekParity", WeekParityComboBox.SelectedIndex == 0);
                                command.Parameters.AddWithValue("@GroupComposition", GroupCompositionComboBox.SelectedIndex == 0);
                                command.Parameters.AddWithValue("@LessonTypeId", (LessonTypeComboBox.SelectedItem as ComboBoxItem)?.Tag);
                                command.Parameters.AddWithValue("@ZanyatieId", zanyatieId);

                                command.ExecuteNonQuery();
                            }

                            // Получаем список новых групп
                            string selectGroupsQuery = @"
                                SELECT id_gruppy 
                                FROM RV_Gruppa 
                                WHERE chetnost_nedeli = @WeekParity 
                                AND polnost_gruppy = @GroupComposition";

                            List<int> groupIds = new List<int>();
                            using (SqlCommand command = new SqlCommand(selectGroupsQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@WeekParity", WeekParityComboBox.SelectedIndex == 0);
                                command.Parameters.AddWithValue("@GroupComposition", GroupCompositionComboBox.SelectedIndex == 0);

                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        groupIds.Add(reader.GetInt32(0));
                                    }
                                }
                            }

                            // Добавляем новые связи с группами
                            string insertRaspisanieQuery = @"
                                INSERT INTO RV_Raspisanie_Gruppy (id_gruppy, id_zanyatia)
                                VALUES (@GroupId, @ZanyatieId)";

                            foreach (int groupId in groupIds)
                            {
                                using (SqlCommand command = new SqlCommand(insertRaspisanieQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@GroupId", groupId);
                                    command.Parameters.AddWithValue("@ZanyatieId", zanyatieId);
                                    command.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            MessageBox.Show("Занятие успешно обновлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            ClearForm();
                            LoadSchedule();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception($"Ошибка при обновлении данных: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRow == null || _selectedRow["ID"] == null)
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
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        using (SqlTransaction transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                // Проверяем, есть ли связи в таблице RV_Administrator
                                string checkAdminQuery = "SELECT COUNT(*) FROM RV_Administrator WHERE id_zanyatia = @ZanyatieId";
                                int adminCount;
                                using (SqlCommand command = new SqlCommand(checkAdminQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@ZanyatieId", Convert.ToInt32(_selectedRow["ID"]));
                                    adminCount = (int)command.ExecuteScalar();
                                }

                                if (adminCount > 0)
                                {
                                    // Сначала удаляем связи в таблице RV_Administrator
                                    string deleteAdminQuery = "UPDATE RV_Administrator SET id_zanyatia = NULL WHERE id_zanyatia = @ZanyatieId";
                                    using (SqlCommand command = new SqlCommand(deleteAdminQuery, connection, transaction))
                                    {
                                        command.Parameters.AddWithValue("@ZanyatieId", Convert.ToInt32(_selectedRow["ID"]));
                                        command.ExecuteNonQuery();
                                    }
                                }

                                // Удаляем связи в расписании групп
                                string deleteRaspisanieQuery = "DELETE FROM RV_Raspisanie_Gruppy WHERE id_zanyatia = @ZanyatieId";
                                using (SqlCommand command = new SqlCommand(deleteRaspisanieQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@ZanyatieId", Convert.ToInt32(_selectedRow["ID"]));
                                    command.ExecuteNonQuery();
                                }

                                // Удаляем само занятие
                                string deleteZanyatieQuery = "DELETE FROM RV_Zanyatie WHERE id_zanyatia = @ZanyatieId";
                                using (SqlCommand command = new SqlCommand(deleteZanyatieQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@ZanyatieId", Convert.ToInt32(_selectedRow["ID"]));
                                    command.ExecuteNonQuery();
                                }

                                transaction.Commit();
                                MessageBox.Show("Занятие успешно удалено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadSchedule();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                throw new Exception($"Ошибка при удалении данных: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSchedule();
        }

        private void LoadGroups()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT 
                                    g.nazvanie_gruppy as [Группа],
                                    f.nazvanie_instituta as [Факультет],
                                    g.nomer_kursa as [Курс],
                                    CASE 
                                        WHEN g.polnost_gruppy = 1 THEN 'Полная группа'
                                        ELSE 'Подгруппа'
                                    END as [Состав группы],
                                    CASE 
                                        WHEN g.chetnost_nedeli = 1 THEN 'Числитель'
                                        ELSE 'Знаменатель'
                                    END as [Четность недели]
                                FROM RV_Gruppa g
                                JOIN RV_Fakultet f ON g.id_fakultet = f.id_fakultet
                                ORDER BY g.nazvanie_gruppy";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        GroupsDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке групп: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveGroupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateGroupInput()) return;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"INSERT INTO RV_Gruppa 
                                   (nazvanie_gruppy, id_fakultet, nomer_kursa, chetnost_nedeli, polnost_gruppy) 
                                   VALUES 
                                   (@GroupName, @FacultyId, @CourseNumber, @WeekParity, @GroupComposition)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@GroupName", GroupNameTextBox.Text);
                        command.Parameters.AddWithValue("@FacultyId", (FacultyComboBox.SelectedItem as ComboBoxItem)?.Tag);
                        command.Parameters.AddWithValue("@CourseNumber", int.Parse(CourseNumberTextBox.Text));
                        command.Parameters.AddWithValue("@WeekParity", GroupFormWeekParityComboBox.SelectedIndex == 0);
                        command.Parameters.AddWithValue("@GroupComposition", GroupFormCompositionComboBox.SelectedIndex == 0);

                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Группа успешно добавлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearGroupForm();
                    LoadGroups();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении группы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            ClearGroupForm();
        }

        private void EditGroupButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedRow = GroupsDataGrid.SelectedItem as DataRowView;
            if (selectedRow == null)
            {
                MessageBox.Show("Выберите группу для редактирования", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"UPDATE RV_Gruppa 
                           SET nazvanie_gruppy = @GroupName, 
                               id_fakultet = @FacultyId, 
                               nomer_kursa = @CourseNumber 
                           WHERE id_gruppy = @GroupId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@GroupName", GroupNameTextBox.Text);
                        command.Parameters.AddWithValue("@FacultyId", (FacultyComboBox.SelectedItem as ComboBoxItem)?.Tag);
                        command.Parameters.AddWithValue("@CourseNumber", int.Parse(CourseNumberTextBox.Text));
                        command.Parameters.AddWithValue("@GroupId", Convert.ToInt32(selectedRow["id_gruppy"]));

                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Группа успешно обновлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearGroupForm();
                    LoadGroups();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении группы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteGroupButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedRow = GroupsDataGrid.SelectedItem as DataRowView;
            if (selectedRow == null)
            {
                MessageBox.Show("Выберите группу для удаления", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить эту группу?", "Подтверждение",
                               MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        using (SqlTransaction transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                // Удаляем связи в расписании
                                string deleteScheduleQuery = "DELETE FROM RV_Raspisanie_Gruppy WHERE id_gruppy = @GroupId";
                                using (SqlCommand command = new SqlCommand(deleteScheduleQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@GroupId", Convert.ToInt32(selectedRow["id_gruppy"]));
                                    command.ExecuteNonQuery();
                                }

                                // Удаляем группу
                                string deleteGroupQuery = "DELETE FROM RV_Gruppa WHERE id_gruppy = @GroupId";
                                using (SqlCommand command = new SqlCommand(deleteGroupQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@GroupId", Convert.ToInt32(selectedRow["id_gruppy"]));
                                    command.ExecuteNonQuery();
                                }

                                transaction.Commit();
                                MessageBox.Show("Группа успешно удалена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadGroups();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                throw new Exception($"Ошибка при удалении группы: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshGroupsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadGroups();
        }

        private bool ValidateGroupInput()
        {
            if (string.IsNullOrWhiteSpace(GroupNameTextBox.Text) ||
                FacultyComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(CourseNumberTextBox.Text) ||
                GroupFormCompositionComboBox.SelectedItem == null ||
                GroupFormWeekParityComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(CourseNumberTextBox.Text, out int courseNumber) || courseNumber < 1 || courseNumber > 6)
            {
                MessageBox.Show("Номер курса должен быть числом от 1 до 6", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ClearGroupForm()
        {
            GroupNameTextBox.Clear();
            FacultyComboBox.SelectedIndex = -1;
            CourseNumberTextBox.Clear();
            GroupFormCompositionComboBox.SelectedIndex = -1;
            GroupFormWeekParityComboBox.SelectedIndex = -1;
        }

        private void LoadTeachers()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT 
                                    familia as [Фамилия],
                                    imya as [Имя],
                                    otchestvo as [Отчество],
                                    email as [Email]
                                FROM RV_Prepodavatel
                                ORDER BY familia, imya, otchestvo";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        TeachersDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке преподавателей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveTeacherButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateTeacherInput()) return;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"INSERT INTO RV_Prepodavatel (familia, imya, otchestvo) 
                           VALUES (@LastName, @FirstName, @MiddleName)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LastName", TeacherLastNameTextBox.Text);
                        command.Parameters.AddWithValue("@FirstName", TeacherFirstNameTextBox.Text);
                        command.Parameters.AddWithValue("@MiddleName", TeacherMiddleNameTextBox.Text);

                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Преподаватель успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearTeacherForm();
                    LoadTeachers();
                    LoadComboBoxes(); // Обновляем список преподавателей в комбобоксе на вкладке расписания
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении преподавателя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddTeacherButton_Click(object sender, RoutedEventArgs e)
        {
            ClearTeacherForm();
        }

        private void EditTeacherButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedRow = TeachersDataGrid.SelectedItem as DataRowView;
            if (selectedRow == null)
            {
                MessageBox.Show("Выберите преподавателя для редактирования", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"UPDATE RV_Prepodavatel 
                           SET familia = @LastName, 
                               imya = @FirstName, 
                               otchestvo = @MiddleName 
                           WHERE id_prepodavatelya = @TeacherId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LastName", TeacherLastNameTextBox.Text);
                        command.Parameters.AddWithValue("@FirstName", TeacherFirstNameTextBox.Text);
                        command.Parameters.AddWithValue("@MiddleName", TeacherMiddleNameTextBox.Text);
                        command.Parameters.AddWithValue("@TeacherId", Convert.ToInt32(selectedRow["id_prepodavatelya"]));

                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Преподаватель успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearTeacherForm();
                    LoadTeachers();
                    LoadComboBoxes(); // Обновляем список преподавателей в комбобоксе на вкладке расписания
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении преподавателя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteTeacherButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedRow = TeachersDataGrid.SelectedItem as DataRowView;
            if (selectedRow == null)
            {
                MessageBox.Show("Выберите преподавателя для удаления", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить этого преподавателя?", "Подтверждение",
                               MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        using (SqlTransaction transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                int teacherId = Convert.ToInt32(selectedRow["id_prepodavatelya"]);

                                // Проверяем, есть ли связанные занятия
                                string checkQuery = "SELECT COUNT(*) FROM RV_Zanyatie WHERE id_prepodavatelya = @TeacherId";
                                using (SqlCommand command = new SqlCommand(checkQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@TeacherId", teacherId);
                                    int count = (int)command.ExecuteScalar();
                                    if (count > 0)
                                    {
                                        throw new Exception("Невозможно удалить преподавателя, так как у него есть назначенные занятия");
                                    }
                                }

                                // Удаляем преподавателя
                                string deleteQuery = "DELETE FROM RV_Prepodavatel WHERE id_prepodavatelya = @TeacherId";
                                using (SqlCommand command = new SqlCommand(deleteQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@TeacherId", teacherId);
                                    command.ExecuteNonQuery();
                                }

                                transaction.Commit();
                                MessageBox.Show("Преподаватель успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadTeachers();
                                LoadComboBoxes(); // Обновляем список преподавателей в комбобоксе на вкладке расписания
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                throw new Exception($"Ошибка при удалении преподавателя: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshTeachersButton_Click(object sender, RoutedEventArgs e)
        {
            LoadTeachers();
        }

        private bool ValidateTeacherInput()
        {
            if (string.IsNullOrWhiteSpace(TeacherLastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(TeacherFirstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(TeacherMiddleNameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ClearTeacherForm()
        {
            TeacherLastNameTextBox.Clear();
            TeacherFirstNameTextBox.Clear();
            TeacherMiddleNameTextBox.Clear();
        }

        private void LoadSubjects()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT 
                                    p.nazvanie_predmeta as [Название предмета],
                                    CONCAT(pr.familia, ' ', pr.imya, ' ', pr.otchestvo) as [Преподаватель]
                                FROM RV_Predmet p
                                LEFT JOIN RV_Zanyatie z ON p.id_predmeta = z.id_predmeta
                                LEFT JOIN RV_Prepodavatel pr ON z.id_prepodavatelya = pr.id_prepodavatelya
                                ORDER BY p.nazvanie_predmeta";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        SubjectsDataGrid.ItemsSource = dataTable.DefaultView;
                    }

                    // Загружаем список преподавателей для ComboBox
                    string teachersQuery = @"SELECT id_prepodavatelya, 
                                           CONCAT(familia, ' ', imya, ' ', otchestvo) as fio 
                                           FROM RV_Prepodavatel 
                                           ORDER BY familia, imya, otchestvo";

                    using (SqlCommand command = new SqlCommand(teachersQuery, connection))
                    {
                        SubjectTeacherComboBox.Items.Clear();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ComboBoxItem item = new ComboBoxItem
                                {
                                    Content = reader["fio"].ToString(),
                                    Tag = reader["id_prepodavatelya"]
                                };
                                SubjectTeacherComboBox.Items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке предметов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSubjectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateSubjectInput()) return;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO RV_Predmet (nazvanie_predmeta) VALUES (@SubjectName)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SubjectName", SubjectNameTextBox.Text);
                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Предмет успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearSubjectForm();
                    LoadSubjects();
                    LoadComboBoxes(); // Обновляем список предметов в комбобоксе на вкладке расписания
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении предмета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddSubjectButton_Click(object sender, RoutedEventArgs e)
        {
            ClearSubjectForm();
        }

        private void EditSubjectButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedRow = SubjectsDataGrid.SelectedItem as DataRowView;
            if (selectedRow == null)
            {
                MessageBox.Show("Выберите предмет для редактирования", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE RV_Predmet SET nazvanie_predmeta = @SubjectName WHERE id_predmeta = @SubjectId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SubjectName", SubjectNameTextBox.Text);
                        command.Parameters.AddWithValue("@SubjectId", Convert.ToInt32(selectedRow["id_predmeta"]));

                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Предмет успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearSubjectForm();
                    LoadSubjects();
                    LoadComboBoxes(); // Обновляем список предметов в комбобоксе на вкладке расписания
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении предмета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteSubjectButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedRow = SubjectsDataGrid.SelectedItem as DataRowView;
            if (selectedRow == null)
            {
                MessageBox.Show("Выберите предмет для удаления", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить этот предмет?", "Подтверждение",
                               MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        using (SqlTransaction transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                int subjectId = Convert.ToInt32(selectedRow["id_predmeta"]);

                                // Проверяем, есть ли связанные занятия
                                string checkQuery = "SELECT COUNT(*) FROM RV_Zanyatie WHERE id_predmeta = @SubjectId";
                                using (SqlCommand command = new SqlCommand(checkQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@SubjectId", subjectId);
                                    int count = (int)command.ExecuteScalar();
                                    if (count > 0)
                                    {
                                        throw new Exception("Невозможно удалить предмет, так как он используется в расписании");
                                    }
                                }

                                // Удаляем предмет
                                string deleteQuery = "DELETE FROM RV_Predmet WHERE id_predmeta = @SubjectId";
                                using (SqlCommand command = new SqlCommand(deleteQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@SubjectId", subjectId);
                                    command.ExecuteNonQuery();
                                }

                                transaction.Commit();
                                MessageBox.Show("Предмет успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadSubjects();
                                LoadComboBoxes(); // Обновляем список предметов в комбобоксе на вкладке расписания
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                throw new Exception($"Ошибка при удалении предмета: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshSubjectsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSubjects();
        }

        private bool ValidateSubjectInput()
        {
            if (string.IsNullOrWhiteSpace(SubjectNameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите название предмета", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ClearSubjectForm()
        {
            SubjectNameTextBox.Clear();
        }

        private void LoadClassrooms()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT 
                                    nazvanie_auditorii as [Аудитория]
                                FROM RV_Auditoria
                                ORDER BY nazvanie_auditorii";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        ClassroomsDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке аудиторий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveClassroomButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateClassroomInput()) return;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO RV_Auditoria (nazvanie_auditorii) VALUES (@ClassroomName)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ClassroomName", ClassroomNameTextBox.Text);
                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Аудитория успешно добавлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearClassroomForm();
                    LoadClassrooms();
                    LoadComboBoxes(); // Обновляем список аудиторий в комбобоксе на вкладке расписания
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении аудитории: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddClassroomButton_Click(object sender, RoutedEventArgs e)
        {
            ClearClassroomForm();
        }

        private void EditClassroomButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedRow = ClassroomsDataGrid.SelectedItem as DataRowView;
            if (selectedRow == null)
            {
                MessageBox.Show("Выберите аудиторию для редактирования", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE RV_Auditoria SET nazvanie_auditorii = @ClassroomName WHERE id_auditorii = @ClassroomId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ClassroomName", ClassroomNameTextBox.Text);
                        command.Parameters.AddWithValue("@ClassroomId", Convert.ToInt32(selectedRow["id_auditorii"]));

                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Аудитория успешно обновлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearClassroomForm();
                    LoadClassrooms();
                    LoadComboBoxes(); // Обновляем список аудиторий в комбобоксе на вкладке расписания
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении аудитории: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteClassroomButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedRow = ClassroomsDataGrid.SelectedItem as DataRowView;
            if (selectedRow == null)
            {
                MessageBox.Show("Выберите аудиторию для удаления", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить эту аудиторию?", "Подтверждение",
                               MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        using (SqlTransaction transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                int classroomId = Convert.ToInt32(selectedRow["id_auditorii"]);

                                // Проверяем, есть ли связанные занятия
                                string checkQuery = "SELECT COUNT(*) FROM RV_Zanyatie WHERE id_auditorii = @ClassroomId";
                                using (SqlCommand command = new SqlCommand(checkQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@ClassroomId", classroomId);
                                    int count = (int)command.ExecuteScalar();
                                    if (count > 0)
                                    {
                                        throw new Exception("Невозможно удалить аудиторию, так как она используется в расписании");
                                    }
                                }

                                // Удаляем аудиторию
                                string deleteQuery = "DELETE FROM RV_Auditoria WHERE id_auditorii = @ClassroomId";
                                using (SqlCommand command = new SqlCommand(deleteQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@ClassroomId", classroomId);
                                    command.ExecuteNonQuery();
                                }

                                transaction.Commit();
                                MessageBox.Show("Аудитория успешно удалена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadClassrooms();
                                LoadComboBoxes(); // Обновляем список аудиторий в комбобоксе на вкладке расписания
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                throw new Exception($"Ошибка при удалении аудитории: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshClassroomsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadClassrooms();
        }

        private bool ValidateClassroomInput()
        {
            if (string.IsNullOrWhiteSpace(ClassroomNameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите номер аудитории", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ClearClassroomForm()
        {
            ClassroomNameTextBox.Clear();
        }

        private void GroupsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRow = GroupsDataGrid.SelectedItem as DataRowView;
            if (selectedRow != null)
            {
                GroupNameTextBox.Text = selectedRow["Группа"].ToString();
                CourseNumberTextBox.Text = selectedRow["Курс"].ToString();

                // Выбираем соответствующий факультет в ComboBox
                foreach (ComboBoxItem item in FacultyComboBox.Items)
                {
                    if (item.Content.ToString() == selectedRow["Факультет"].ToString())
                    {
                        FacultyComboBox.SelectedItem = item;
                        break;
                    }
                }

                // Устанавливаем состав группы
                GroupFormCompositionComboBox.SelectedIndex =
                    selectedRow["Состав группы"].ToString() == "Полная группа" ? 0 : 1;

                // Устанавливаем четность недели
                GroupFormWeekParityComboBox.SelectedIndex =
                    selectedRow["Четность недели"].ToString() == "Числитель" ? 0 : 1;
            }
        }

        private void TeachersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRow = TeachersDataGrid.SelectedItem as DataRowView;
            if (selectedRow != null)
            {
                TeacherLastNameTextBox.Text = selectedRow["Фамилия"].ToString();
                TeacherFirstNameTextBox.Text = selectedRow["Имя"].ToString();
                TeacherMiddleNameTextBox.Text = selectedRow["Отчество"].ToString();
                TeacherEmailTextBox.Text = selectedRow["Email"].ToString();
            }
        }

        private void SubjectsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRow = SubjectsDataGrid.SelectedItem as DataRowView;
            if (selectedRow != null)
            {
                SubjectNameTextBox.Text = selectedRow["Название предмета"].ToString();

                // Выбираем соответствующего преподавателя
                string teacherName = selectedRow["Преподаватель"].ToString();
                foreach (ComboBoxItem item in SubjectTeacherComboBox.Items)
                {
                    if (item.Content.ToString() == teacherName)
                    {
                        SubjectTeacherComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void ClassroomsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRow = ClassroomsDataGrid.SelectedItem as DataRowView;
            if (selectedRow != null)
            {
                ClassroomNameTextBox.Text = selectedRow["Аудитория"].ToString();
            }
        }

        private void LoadFaculties()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT id_fakultet, nazvanie_instituta FROM RV_Fakultet ORDER BY nazvanie_instituta";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            FacultyComboBox.Items.Clear();
                            while (reader.Read())
                            {
                                ComboBoxItem item = new ComboBoxItem
                                {
                                    Content = reader["nazvanie_instituta"].ToString(),
                                    Tag = reader["id_fakultet"]
                                };
                                FacultyComboBox.Items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке факультетов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
