using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;
using System.Configuration;
using System.Windows.Controls;
using System.Collections.Generic;

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
                    string query = @"SELECT 
                                    z.id_zanyatia AS [ID],
                                    g.nazvanie_gruppy AS [Группа],
                                    dn.nazvanie AS [День недели],
                                    z.nomer_pary AS [Номер пары],
                                    p.nazvanie_predmeta AS [Предмет],
                                    tz.nazvanie AS [Тип занятия],
                                    CONCAT(pr.familia, ' ', pr.imya, ' ', pr.otchestvo) AS [Преподаватель],
                                    a.nazvanie_auditorii AS [Аудитория],
                                    CASE 
                                        WHEN z.chetnost_nedeli = 1 THEN 'Числитель'
                                        ELSE 'Знаменатель'
                                    END AS [Четность недели],
                                    CASE 
                                        WHEN z.polnost_gruppy = 1 THEN 'Полная группа'
                                        ELSE 'Подгруппа'
                                    END AS [Состав группы]
                                FROM RV_Raspisanie_Gruppy rg
                                JOIN RV_Gruppa g ON rg.id_gruppy = g.id_gruppy
                                JOIN RV_Zanyatie z ON rg.id_zanyatia = z.id_zanyatia
                                JOIN RV_Den_Nedeli dn ON z.id_den_nedeli = dn.id_den_nedeli
                                JOIN RV_Predmet p ON z.id_predmeta = p.id_predmeta
                                JOIN RV_Tip_Zanyatia tz ON z.id_tip_zanyatia = tz.id_tip_zanyatia
                                JOIN RV_Prepodavatel pr ON z.id_prepodavatelya = pr.id_prepodavatelya
                                JOIN RV_Auditoria a ON z.id_auditorii = a.id_auditorii
                                ORDER BY 
                                    CASE dn.nazvanie
                                        WHEN 'Понедельник' THEN 1
                                        WHEN 'Вторник' THEN 2
                                        WHEN 'Среда' THEN 3
                                        WHEN 'Четверг' THEN 4
                                        WHEN 'Пятница' THEN 5
                                        WHEN 'Суббота' THEN 6
                                        WHEN 'Воскресенье' THEN 7
                                    END,
                                    z.nomer_pary";

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
    }
}
