using System;
using System.Data;
using System.Windows;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;

namespace Vuz_Shedule
{
    public partial class StudentWindow : Window
    {
        private string connectionString = "data source=stud-mssql.sttec.yar.ru,38325;user id=user122_db;password=user122;MultipleActiveResultSets=True;App=EntityFramework";

        public StudentWindow()
        {
            InitializeComponent();
            LoadFaculties();
            InitializeCourseComboBox();
        }

        private void LoadFaculties()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT nazvanie_instituta FROM RV_Fakultet";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                FacultyComboBox.Items.Add(reader["nazvanie_instituta"].ToString());
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

        private void InitializeCourseComboBox()
        {
            for (int i = 1; i <= 6; i++)
            {
                CourseComboBox.Items.Add(i);
            }
        }

        private void FacultyComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (FacultyComboBox.SelectedItem != null && CourseComboBox.SelectedItem != null)
            {
                LoadGroups();
            }
        }

        private void CourseComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (FacultyComboBox.SelectedItem != null && CourseComboBox.SelectedItem != null)
            {
                LoadGroups();
            }
        }

        private void LoadGroups()
        {
            try
            {
                GroupComboBox.Items.Clear();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT g.nazvanie_gruppy 
                        FROM RV_Gruppa g
                        JOIN RV_Fakultet f ON g.id_fakultet = f.id_fakultet
                        WHERE f.nazvanie_instituta = @Faculty 
                        AND g.nomer_kursa = @Course";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Faculty", FacultyComboBox.SelectedItem.ToString());
                        command.Parameters.AddWithValue("@Course", CourseComboBox.SelectedItem);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                GroupComboBox.Items.Add(reader["nazvanie_gruppy"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке групп: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            if (GroupComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите группу", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT * FROM RV_Raspisanie_Polnoe
                        WHERE [Группа] = @GroupName
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
                        command.Parameters.AddWithValue("@GroupName", GroupComboBox.SelectedItem.ToString());
                        
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
    }
} 