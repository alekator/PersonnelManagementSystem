using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PersonnelManagementSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Database _database;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Load; // Подключаем событие загрузки окна
        }

        /// <summary>
        /// Событие загрузки главного окна.
        /// </summary>
        private void MainWindow_Load(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.WriteLog("MainWindow_Load: Загрузка главного окна началась.");

                if (string.IsNullOrWhiteSpace(DatabaseSettings.ConnectionString))
                {
                    Logger.WriteLog("MainWindow_Load: Строка подключения пустая. Открываем окно настроек подключения.");
                    MessageBox.Show("Настройте подключение к базе данных перед началом работы.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);

                    ConnectionSettingsWindow settingsWindow = new ConnectionSettingsWindow();
                    settingsWindow.ShowDialog();
                }

                if (!string.IsNullOrWhiteSpace(DatabaseSettings.ConnectionString))
                {
                    Logger.WriteLog($"MainWindow_Load: Используется строка подключения: {DatabaseSettings.ConnectionString}");

                    using (SqlConnection connection = new SqlConnection(DatabaseSettings.ConnectionString))
                    {
                        Logger.WriteLog("MainWindow_Load: Попытка подключения к базе данных...");
                        connection.Open();
                        Logger.WriteLog("MainWindow_Load: Подключение успешно установлено.");
                        MessageBox.Show("Подключение к базе данных успешно установлено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    Logger.WriteLog($"MainWindow_Load: Перед созданием Database строка подключения: {DatabaseSettings.ConnectionString}");
                    _database = new Database(DatabaseSettings.ConnectionString);
                    Logger.WriteLog("MainWindow_Load: Объект Database успешно инициализирован.");

                    DataTable organizations = _database.ExecuteSelectQuery("SELECT * FROM Organizations");
                    Logger.WriteLog($"MainWindow_Load: Загружено {organizations.Rows.Count} организаций из базы данных.");
                    LoadOrganizationsToUI(organizations);
                }
                else
                {
                    Logger.WriteLog("MainWindow_Load: Строка подключения осталась пустой после окна настроек.");
                }
            }
            catch (SqlException sqlEx)
            {
                Logger.WriteLog($"MainWindow_Load: SqlException. Подробности: {sqlEx.Message} (Код ошибки: {sqlEx.Number})");
                MessageBox.Show($"Ошибка подключения к базе данных:\nКод ошибки: {sqlEx.Number}\nСообщение: {sqlEx.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidOperationException invEx)
            {
                Logger.WriteLog($"MainWindow_Load: InvalidOperationException. Подробности: {invEx.Message}");
                MessageBox.Show($"Ошибка: {invEx.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"MainWindow_Load: Exception. Подробности: {ex.Message}");
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Logger.WriteLog("MainWindow_Load: Загрузка главного окна завершена.");
            }
        }





        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            ConnectionSettingsWindow settingsWindow = new ConnectionSettingsWindow();
            settingsWindow.ShowDialog();
        }


        /// <summary>
        /// Загружает список организаций в интерфейс.
        /// </summary>
        /// <param name="organizations">Таблица с данными организаций.</param>
        private void LoadOrganizationsToUI(DataTable organizations)
        {
            try
            {
                // Привязываем данные из DataTable к DataGrid
                DataGridOrganizations.ItemsSource = organizations.DefaultView;
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Ошибка загрузки организаций в интерфейс: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку "Добавить организацию".
        /// </summary>
        private void BtnAddOrganization_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string query = "INSERT INTO Organizations (Name, Address, Phone) VALUES (@Name, @Address, @Phone)";
                var random = new Random();
                var parameters = new[]
                {
            new System.Data.SqlClient.SqlParameter("@Name", $"Организация {random.Next(1, 1000)}"),
            new System.Data.SqlClient.SqlParameter("@Address", $"Адрес {random.Next(1, 100)}"),
            new System.Data.SqlClient.SqlParameter("@Phone", $"123-{random.Next(100, 999)}-{random.Next(1000, 9999)}")
        };

                _database.ExecuteNonQuery(query, parameters);
                Logger.WriteLog("Добавлена новая организация.");

                // Обновляем список организаций
                DataTable organizations = _database.ExecuteSelectQuery("SELECT * FROM Organizations");
                LoadOrganizationsToUI(organizations);
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Ошибка добавления организации: {ex.Message}");
                MessageBox.Show($"Ошибка добавления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnEditOrganization_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridOrganizations.SelectedItem is DataRowView selectedRow)
            {
                int organizationId = Convert.ToInt32(selectedRow["Id"]);
                string newName = "Обновленная Организация";
                string query = "UPDATE Organizations SET Name = @Name WHERE Id = @Id";

                var parameters = new[]
                {
            new System.Data.SqlClient.SqlParameter("@Name", newName),
            new System.Data.SqlClient.SqlParameter("@Id", organizationId)
        };

                _database.ExecuteNonQuery(query, parameters);
                Logger.WriteLog($"Организация с ID {organizationId} обновлена.");

                // Обновляем список организаций
                DataTable organizations = _database.ExecuteSelectQuery("SELECT * FROM Organizations");
                LoadOrganizationsToUI(organizations);
            }
        }

        private void BtnDeleteOrganization_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridOrganizations.SelectedItem is DataRowView selectedRow)
            {
                int organizationId = Convert.ToInt32(selectedRow["Id"]);
                string query = "DELETE FROM Organizations WHERE Id = @Id";

                var parameters = new[]
                {
            new System.Data.SqlClient.SqlParameter("@Id", organizationId)
        };

                _database.ExecuteNonQuery(query, parameters);
                Logger.WriteLog($"Организация с ID {organizationId} удалена.");

                // Обновляем список организаций
                DataTable organizations = _database.ExecuteSelectQuery("SELECT * FROM Organizations");
                LoadOrganizationsToUI(organizations);
            }
        }
        private void DataGridOrganizations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataGridOrganizations.SelectedItem is DataRowView selectedRow)
            {
                int organizationId = Convert.ToInt32(selectedRow["Id"]);
                LoadEmployees(organizationId);
            }
        }

        private void LoadEmployees(int organizationId)
        {
            try
            {
                string query = "SELECT Id, FirstName, LastName, Position, Photo FROM Employees WHERE OrganizationId = @OrganizationId";
                var parameters = new[]
                {
            new System.Data.SqlClient.SqlParameter("@OrganizationId", organizationId)
        };

                DataTable employees = _database.ExecuteSelectQuery(query, parameters);
                DataGridEmployees.ItemsSource = employees.DefaultView;
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Ошибка загрузки сотрудников: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки сотрудников: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridOrganizations.SelectedItem is DataRowView selectedOrganization)
            {
                int organizationId = Convert.ToInt32(selectedOrganization["Id"]);

                string query = "INSERT INTO Employees (OrganizationId, FirstName, LastName, Position) VALUES (@OrganizationId, @FirstName, @LastName, @Position)";
                var parameters = new[]
                {
            new System.Data.SqlClient.SqlParameter("@OrganizationId", organizationId),
            new System.Data.SqlClient.SqlParameter("@FirstName", "Иван"),
            new System.Data.SqlClient.SqlParameter("@LastName", "Иванов"),
            new System.Data.SqlClient.SqlParameter("@Position", "Менеджер")
        };

                _database.ExecuteNonQuery(query, parameters);
                Logger.WriteLog("Добавлен новый сотрудник.");
                LoadEmployees(organizationId);
            }
            else
            {
                MessageBox.Show("Выберите организацию для добавления сотрудника.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void BtnEditEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridEmployees.SelectedItem is DataRowView selectedEmployee)
            {
                int employeeId = Convert.ToInt32(selectedEmployee["Id"]);
                string query = "UPDATE Employees SET FirstName = @FirstName, LastName = @LastName, Position = @Position WHERE Id = @Id";

                var parameters = new[]
                {
            new System.Data.SqlClient.SqlParameter("@FirstName", "Петр"),
            new System.Data.SqlClient.SqlParameter("@LastName", "Петров"),
            new System.Data.SqlClient.SqlParameter("@Position", "Разработчик"),
            new System.Data.SqlClient.SqlParameter("@Id", employeeId)
        };

                _database.ExecuteNonQuery(query, parameters);
                Logger.WriteLog($"Сотрудник с ID {employeeId} обновлен.");

                if (DataGridOrganizations.SelectedItem is DataRowView selectedOrganization)
                {
                    int organizationId = Convert.ToInt32(selectedOrganization["Id"]);
                    LoadEmployees(organizationId);
                }
            }
        }
        private void BtnDeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridEmployees.SelectedItem is DataRowView selectedEmployee)
            {
                int employeeId = Convert.ToInt32(selectedEmployee["Id"]);
                string query = "DELETE FROM Employees WHERE Id = @Id";

                var parameters = new[]
                {
            new System.Data.SqlClient.SqlParameter("@Id", employeeId)
        };

                _database.ExecuteNonQuery(query, parameters);
                Logger.WriteLog($"Сотрудник с ID {employeeId} удален.");

                if (DataGridOrganizations.SelectedItem is DataRowView selectedOrganization)
                {
                    int organizationId = Convert.ToInt32(selectedOrganization["Id"]);
                    LoadEmployees(organizationId);
                }
            }
        }
        private void BtnUploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridEmployees.SelectedItem is DataRowView selectedEmployee)
            {
                // Открытие диалога для выбора файла
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        // Чтение файла в массив байтов
                        byte[] photoBytes = File.ReadAllBytes(openFileDialog.FileName);

                        // Получение ID сотрудника
                        int employeeId = Convert.ToInt32(selectedEmployee["Id"]);

                        // Обновление фото в базе данных
                        string query = "UPDATE Employees SET Photo = @Photo WHERE Id = @Id";
                        var parameters = new[]
                        {
                    new System.Data.SqlClient.SqlParameter("@Photo", photoBytes),
                    new System.Data.SqlClient.SqlParameter("@Id", employeeId)
                };

                        _database.ExecuteNonQuery(query, parameters);
                        Logger.WriteLog($"Фотография сотрудника с ID {employeeId} обновлена.");

                        // Отображение загруженного фото
                        using (MemoryStream stream = new MemoryStream(photoBytes))
                        {
                            BitmapImage image = new BitmapImage();
                            image.BeginInit();
                            image.StreamSource = stream;
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.EndInit();
                            EmployeePhoto.Source = image;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteLog($"Ошибка при загрузке фотографии: {ex.Message}");
                        MessageBox.Show($"Ошибка загрузки фотографии: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите сотрудника для загрузки фото.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void DataGridEmployees_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataGridEmployees.SelectedItem is DataRowView selectedEmployee)
            {
                if (selectedEmployee["Photo"] != DBNull.Value)
                {
                    byte[] photoBytes = (byte[])selectedEmployee["Photo"];
                    using (MemoryStream stream = new MemoryStream(photoBytes))
                    {
                        BitmapImage image = new BitmapImage();
                        image.BeginInit();
                        image.StreamSource = stream;
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.EndInit();
                        EmployeePhoto.Source = image;
                    }
                }
                else
                {
                    // Если фото нет, очищаем Image
                    EmployeePhoto.Source = null;
                }
            }
        }


    }
}
