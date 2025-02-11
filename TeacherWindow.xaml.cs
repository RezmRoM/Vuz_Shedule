using System;
using System.Data;
using System.Windows;
using System.Data.SqlClient;
using System.Configuration;

namespace Vuz_Shedule
{
    public partial class TeacherWindow : Window
    {
        private string connectionString = "data source=stud-mssql.sttec.yar.ru,38325;user id=user122_db;password=user122;MultipleActiveResultSets=True;App=EntityFramework";
        private readonly string _teacherEmail;
        private string _teacherFullName;

        public TeacherWindow(string teacherEmail)
        {
            InitializeComponent();
            _teacherEmail = teacherEmail;
            LoadTeacherInfo();
            LoadAllData();
        }

        private void LoadTeacherInfo()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT CONCAT(familia, ' ', imya, ' ', otchestvo) as FullName 
                                   FROM RV_Prepodavatel 
                                   WHERE email = @TeacherEmail";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TeacherEmail", _teacherEmail);
                        _teacherFullName = command.ExecuteScalar()?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке информации о преподавателе: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAllData()
        {
            LoadTeacherSchedule();
            LoadTeacherGroups();
            LoadTeacherSubjects();
            LoadTeacherClassrooms();
        }

        private void LoadTeacherSchedule()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT * FROM RV_Raspisanie_Polnoe 
                                   WHERE [Преподаватель] = @TeacherName
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
                        command.Parameters.AddWithValue("@TeacherName", _teacherFullName);
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        TeacherScheduleDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке расписания: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTeacherGroups()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT DISTINCT 
                                        g.nazvanie_gruppy as [Группа],
                                        f.nazvanie_instituta as [Факультет],
                                        g.nomer_kursa as [Курс],
                                        CASE 
                                            WHEN g.polnost_gruppy = 1 THEN 'Полная группа'
                                            ELSE 'Подгруппа'
                                        END as [Состав группы]
                                   FROM RV_Raspisanie_Gruppy rg
                                   JOIN RV_Gruppa g ON rg.id_gruppy = g.id_gruppy
                                   JOIN RV_Fakultet f ON g.id_fakultet = f.id_fakultet
                                   JOIN RV_Zanyatie z ON rg.id_zanyatia = z.id_zanyatia
                                   JOIN RV_Prepodavatel p ON z.id_prepodavatelya = p.id_prepodavatelya
                                   WHERE CONCAT(p.familia, ' ', p.imya, ' ', p.otchestvo) = @TeacherName
                                   ORDER BY [Группа]";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TeacherName", _teacherFullName);
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        GroupsDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке групп: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTeacherSubjects()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT 
                                        p.nazvanie_predmeta as [Предмет],
                                        tz.nazvanie as [Тип занятия],
                                        COUNT(DISTINCT rg.id_gruppy) as [Количество групп]
                                   FROM RV_Zanyatie z
                                   JOIN RV_Predmet p ON z.id_predmeta = p.id_predmeta
                                   JOIN RV_Tip_Zanyatia tz ON z.id_tip_zanyatia = tz.id_tip_zanyatia
                                   JOIN RV_Prepodavatel pr ON z.id_prepodavatelya = pr.id_prepodavatelya
                                   JOIN RV_Raspisanie_Gruppy rg ON z.id_zanyatia = rg.id_zanyatia
                                   WHERE CONCAT(pr.familia, ' ', pr.imya, ' ', pr.otchestvo) = @TeacherName
                                   GROUP BY p.nazvanie_predmeta, tz.nazvanie
                                   ORDER BY [Предмет], [Тип занятия]";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TeacherName", _teacherFullName);
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        SubjectsDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке предметов: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTeacherClassrooms()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT DISTINCT
                                a.nazvanie_auditorii as [Аудитория],
                                dn.nazvanie as [День недели],
                                z.nomer_pary as [Номер пары],
                                p.nazvanie_predmeta as [Предмет],
                                tz.nazvanie as [Тип занятия],
                                dn.id_den_nedeli -- Добавлен недостающий столбец для сортировки
                            FROM RV_Zanyatie z
                            JOIN RV_Auditoria a ON z.id_auditorii = a.id_auditorii
                            JOIN RV_Den_Nedeli dn ON z.id_den_nedeli = dn.id_den_nedeli
                            JOIN RV_Predmet p ON z.id_predmeta = p.id_predmeta
                            JOIN RV_Tip_Zanyatia tz ON z.id_tip_zanyatia = tz.id_tip_zanyatia
                            JOIN RV_Prepodavatel pr ON z.id_prepodavatelya = pr.id_prepodavatelya
                            WHERE CONCAT(pr.familia, ' ', pr.imya, ' ', pr.otchestvo) = @TeacherName
                            ORDER BY 
                                dn.id_den_nedeli, -- Сортировка по ID дня
                                z.nomer_pary,
                                a.nazvanie_auditorii";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TeacherName", _teacherFullName);
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        ClassroomsDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке аудиторий: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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