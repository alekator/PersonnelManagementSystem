﻿using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PersonnelManagementSystem
{
    /// <summary>
    /// Класс основного окна приложения
    /// </summary>
    public partial class MainWindow : Window
    {
        private Database _database;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Load;
        }

        /// <summary>
        /// Выполняется при загрузке главного окна.
        /// Проверяет строку подключения, инициирует базу данных и загружает список организаций.
        /// </summary>
        private async void MainWindow_Load(object sender, RoutedEventArgs e)
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
                        await connection.OpenAsync();
                        Logger.WriteLog("MainWindow_Load: Подключение успешно установлено.");
                        MessageBox.Show("Подключение к базе данных успешно установлено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    Logger.WriteLog($"MainWindow_Load: Перед созданием Database строка подключения: {DatabaseSettings.ConnectionString}");
                    _database = new Database(DatabaseSettings.ConnectionString);
                    Logger.WriteLog("MainWindow_Load: Объект Database успешно инициализирован.");

                    DataTable organizations = await _database.ExecuteSelectQueryAsync("SELECT * FROM Organizations");
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

        /// <summary>
        /// Обновляет данные организаций и связанных сотрудников.
        /// </summary>
        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.WriteLog("Обновление данных началось...");
                DataTable organizations = await _database.ExecuteSelectQueryAsync("SELECT * FROM Organizations");
                LoadOrganizationsToUI(organizations);

                if (DataGridOrganizations.SelectedItem is DataRowView selectedOrganization)
                {
                    int organizationId = Convert.ToInt32(selectedOrganization["Id"]);
                    DataTable employees = await _database.ExecuteSelectQueryAsync("SELECT * FROM Employees WHERE OrganizationId = @OrgId",
                        new[] { new SqlParameter("@OrgId", organizationId) });
                    DataGridEmployees.ItemsSource = employees.DefaultView;
                }
                Logger.WriteLog("Обновление данных завершено.");
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Ошибка при обновлении данных: {ex.Message}");
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Открывает окно настроек подключения.
        /// </summary>
        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            ConnectionSettingsWindow settingsWindow = new ConnectionSettingsWindow();
            settingsWindow.ShowDialog();
        }


        /// <summary>
        /// Отображает список организаций в интерфейсе.
        /// </summary>
        private void LoadOrganizationsToUI(DataTable organizations)
        {
            try
            {
                DataGridOrganizations.ItemsSource = organizations.DefaultView;
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Ошибка загрузки организаций в интерфейс: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Добавляет новую организацию.
        /// </summary>
        private async void BtnAddOrganization_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var editWindow = new OrganizationEditWindow();
                if (editWindow.ShowDialog() == true)
                {
                    string query = "INSERT INTO Organizations (Name, Address, Phone) VALUES (@Name, @Address, @Phone)";
                    var parameters = new[]
                    {
                new SqlParameter("@Name", editWindow.OrganizationName),
                new SqlParameter("@Address", editWindow.OrganizationAddress),
                new SqlParameter("@Phone", editWindow.OrganizationPhone)
            };

                    await _database.ExecuteNonQueryAsync(query, parameters);
                    Logger.WriteLog($"Организация добавлена: {editWindow.OrganizationName} ({editWindow.OrganizationAddress}, {editWindow.OrganizationPhone})");

                    DataTable organizations = await _database.ExecuteSelectQueryAsync("SELECT * FROM Organizations");
                    LoadOrganizationsToUI(organizations);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Ошибка добавления организации: {ex.Message}");
                MessageBox.Show($"Ошибка добавления организации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /// <summary>
        /// Редактирует выбранную организацию.
        /// </summary>
        private async void BtnEditOrganization_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridOrganizations.SelectedItem is DataRowView selectedRow)
            {
                int organizationId = Convert.ToInt32(selectedRow["Id"]);
                string currentName = selectedRow["Name"].ToString();
                string currentAddress = selectedRow["Address"].ToString();
                string currentPhone = selectedRow["Phone"].ToString();

                var editWindow = new OrganizationEditWindow(currentName, currentAddress, currentPhone);
                if (editWindow.ShowDialog() == true)
                {
                    string query = "UPDATE Organizations SET Name = @Name, Address = @Address, Phone = @Phone WHERE Id = @Id";
                    var parameters = new[]
                    {
                new SqlParameter("@Name", editWindow.OrganizationName),
                new SqlParameter("@Address", editWindow.OrganizationAddress),
                new SqlParameter("@Phone", editWindow.OrganizationPhone),
                new SqlParameter("@Id", organizationId)
            };

                    await _database.ExecuteNonQueryAsync(query, parameters);
                    Logger.WriteLog($"Организация с ID {organizationId} обновлена: {editWindow.OrganizationName}");

                    DataTable organizations = await _database.ExecuteSelectQueryAsync("SELECT * FROM Organizations");
                    LoadOrganizationsToUI(organizations);
                }
            }
            else
            {
                MessageBox.Show("Выберите организацию для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Удаляет выбранную организацию и её сотрудников.
        /// </summary>
        private async void BtnDeleteOrganization_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridOrganizations.SelectedItem is DataRowView selectedRow)
            {
                try
                {
                    int organizationId = Convert.ToInt32(selectedRow["Id"]);
                    string orgName = selectedRow["Name"].ToString();

                    string deleteEmployeesQuery = "DELETE FROM Employees WHERE OrganizationId = @OrgId";
                    string deleteOrganizationQuery = "DELETE FROM Organizations WHERE Id = @Id";

                    var employeeParams = new[] { new SqlParameter("@OrgId", organizationId) };
                    var organizationParams = new[] { new SqlParameter("@Id", organizationId) };

                    await _database.ExecuteNonQueryAsync(deleteEmployeesQuery, employeeParams);
                    await _database.ExecuteNonQueryAsync(deleteOrganizationQuery, organizationParams);

                    Logger.WriteLog($"Удалена организация: {orgName} (ID: {organizationId}) вместе с её сотрудниками.");

                    DataTable organizations = await _database.ExecuteSelectQueryAsync("SELECT * FROM Organizations");
                    LoadOrganizationsToUI(organizations);
                }
                catch (Exception ex)
                {
                    Logger.WriteLog($"Ошибка при удалении организации: {ex.Message}");
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите организацию для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Выполняет поиск организаций по тексту в строке поиска.
        /// </summary>
        private void TxtSearchOrganizations_KeyUp(object sender, KeyEventArgs e)
        {
            if (DataGridOrganizations.ItemsSource is DataView dataView)
            {
                string filterText = TxtSearchOrganizations.Text.ToLower();
                dataView.RowFilter = $"Convert(Id, 'System.String') LIKE '%{filterText}%' OR Name LIKE '%{filterText}%' OR Address LIKE '%{filterText}%' OR Phone LIKE '%{filterText}%'";
            }
        }


        /// <summary>
        /// Обработчик события изменения выбранной строки в таблице организаций.
        /// </summary>
        private void DataGridOrganizations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataGridOrganizations.SelectedItem is DataRowView selectedRow)
            {
                int organizationId = Convert.ToInt32(selectedRow["Id"]);
                LoadEmployees(organizationId);
            }
        }

        /// <summary>
        /// Загружает сотрудников, связанных с выбранной организацией.
        /// </summary>
        private async void LoadEmployees(int organizationId)
        {
            try
            {
                Logger.WriteLog($"LoadEmployees: Загружаем сотрудников для организации ID={organizationId}.");

                string query = "SELECT Id, FirstName, LastName, Position, Photo FROM Employees WHERE OrganizationId = @OrganizationId";
                var parameters = new[]
                {
            new SqlParameter("@OrganizationId", organizationId)
        };

                DataTable employees = await _database.ExecuteSelectQueryAsync(query, parameters);
                Logger.WriteLog($"LoadEmployees: Загружено {employees.Rows.Count} сотрудников.");
                DataGridEmployees.ItemsSource = employees.DefaultView;
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"LoadEmployees: Ошибка загрузки сотрудников: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки сотрудников: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Добавляет нового сотрудника в выбранную организацию.
        /// </summary>
        private async void BtnAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridOrganizations.SelectedItem is DataRowView selectedOrganization)
            {
                int organizationId = Convert.ToInt32(selectedOrganization["Id"]);

                var editWindow = new EmployeeEditWindow();
                if (editWindow.ShowDialog() == true)
                {
                    string query = "INSERT INTO Employees (OrganizationId, FirstName, LastName, Position) VALUES (@OrganizationId, @FirstName, @LastName, @Position)";
                    var parameters = new[]
                    {
                new SqlParameter("@OrganizationId", organizationId),
                new SqlParameter("@FirstName", editWindow.FirstName),
                new SqlParameter("@LastName", editWindow.LastName),
                new SqlParameter("@Position", editWindow.Position)
            };

                    await _database.ExecuteNonQueryAsync(query, parameters);
                    Logger.WriteLog($"Добавлен новый сотрудник: {editWindow.FirstName} {editWindow.LastName} ({editWindow.Position})");

                    LoadEmployees(organizationId);
                }
            }
            else
            {
                MessageBox.Show("Выберите организацию для добавления сотрудника.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Редактирует данные выбранного сотрудника.
        /// </summary>
        private async void BtnEditEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridEmployees.SelectedItem is DataRowView selectedEmployee)
            {
                int employeeId = Convert.ToInt32(selectedEmployee["Id"]);
                string currentFirstName = selectedEmployee["FirstName"].ToString();
                string currentLastName = selectedEmployee["LastName"].ToString();
                string currentPosition = selectedEmployee["Position"].ToString();

                var editWindow = new EmployeeEditWindow(currentFirstName, currentLastName, currentPosition);
                if (editWindow.ShowDialog() == true)
                {
                    string query = "UPDATE Employees SET FirstName = @FirstName, LastName = @LastName, Position = @Position WHERE Id = @Id";
                    var parameters = new[]
                    {
                new SqlParameter("@FirstName", editWindow.FirstName),
                new SqlParameter("@LastName", editWindow.LastName),
                new SqlParameter("@Position", editWindow.Position),
                new SqlParameter("@Id", employeeId)
            };

                    await _database.ExecuteNonQueryAsync(query, parameters);
                    Logger.WriteLog($"Сотрудник с ID {employeeId} обновлен: {editWindow.FirstName} {editWindow.LastName} ({editWindow.Position})");

                    if (DataGridOrganizations.SelectedItem is DataRowView selectedOrganization)
                    {
                        int organizationId = Convert.ToInt32(selectedOrganization["Id"]);
                        LoadEmployees(organizationId);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите сотрудника для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Удаляет выбранного сотрудника.
        /// </summary>
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

        /// <summary>
        /// Загрузка и обрезка фотографии сотрудника перед сохранением в базу данных.
        /// </summary>
        private void BtnUploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridEmployees.SelectedItem is DataRowView selectedEmployee)
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        using (var stream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                        {
                            BitmapImage originalImage = new BitmapImage();
                            originalImage.BeginInit();
                            originalImage.StreamSource = stream;
                            originalImage.CacheOption = BitmapCacheOption.OnLoad;
                            originalImage.EndInit();

                            CroppedBitmap croppedImage = CropToFitAspect(originalImage, 3, 4);

                            byte[] photoBytes = GetImageBytes(croppedImage);

                            int employeeId = Convert.ToInt32(selectedEmployee["Id"]);

                            string query = "UPDATE Employees SET Photo = @Photo WHERE Id = @Id";
                            var parameters = new[]
                            {
                        new System.Data.SqlClient.SqlParameter("@Photo", photoBytes),
                        new System.Data.SqlClient.SqlParameter("@Id", employeeId)
                    };

                            _database.ExecuteNonQuery(query, parameters);
                            Logger.WriteLog($"Фотография сотрудника с ID {employeeId} обновлена.");

                            EmployeePhoto.Source = croppedImage;
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

        /// <summary>
        /// Обрезает изображение до заданных пропорций (3/4).
        /// </summary>
        private CroppedBitmap CropToFitAspect(BitmapImage source, int targetWidthRatio, int targetHeightRatio)
        {
            double sourceWidth = source.PixelWidth;
            double sourceHeight = source.PixelHeight;

            double targetAspectRatio = (double)targetWidthRatio / targetHeightRatio;
            double sourceAspectRatio = sourceWidth / sourceHeight;

            int cropWidth, cropHeight, offsetX, offsetY;

            if (sourceAspectRatio > targetAspectRatio)
            {
                cropHeight = (int)sourceHeight;
                cropWidth = (int)(sourceHeight * targetAspectRatio);
                offsetX = (int)((sourceWidth - cropWidth) / 2);
                offsetY = 0;
            }
            else
            {
                cropWidth = (int)sourceWidth;
                cropHeight = (int)(sourceWidth / targetAspectRatio);
                offsetX = 0;
                offsetY = (int)((sourceHeight - cropHeight) / 2);
            }

            return new CroppedBitmap(source, new Int32Rect(offsetX, offsetY, cropWidth, cropHeight));
        }

        /// <summary>
        /// Преобразует изображение в байтовый массив.
        /// </summary>
        private byte[] GetImageBytes(BitmapSource source)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(stream);
                return stream.ToArray();
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
                    EmployeePhoto.Source = null;
                }
            }
        }

        private void TxtSearchEmployees_KeyUp(object sender, KeyEventArgs e)
        {
            if (DataGridEmployees.ItemsSource is DataView dataView)
            {
                string filterText = TxtSearchEmployees.Text.ToLower();
                dataView.RowFilter = $"Convert(Id, 'System.String') LIKE '%{filterText}%' OR FirstName LIKE '%{filterText}%' OR LastName LIKE '%{filterText}%' OR Position LIKE '%{filterText}%'";
            }
        }


        /// <summary>
        /// Выполняет стресс-тест добавления: 10 организаций по 100 сотрудников в каждой, в базу данных.
        /// </summary>
        private async void BtnStressTest_Click(object sender, RoutedEventArgs e)
        {
            StressTestLog.Clear();
            string startMessage = "Начало стресс-теста...";
            StressTestLog.AppendText(startMessage + "\n");
            Logger.WriteLog(startMessage);

            for (int i = 1; i <= 10; i++)
            {
                string orgName = $"Организация {i}";
                string orgAddress = $"Адрес {i}";
                string orgPhone = $"123-456-78{i:D2}";

                string insertOrgQuery = "INSERT INTO Organizations (Name, Address, Phone) OUTPUT INSERTED.Id VALUES (@Name, @Address, @Phone)";
                var orgParams = new[]
                {
            new SqlParameter("@Name", orgName),
            new SqlParameter("@Address", orgAddress),
            new SqlParameter("@Phone", orgPhone),
        };

                try
                {
                    int organizationId = (int)await _database.ExecuteScalarAsync(insertOrgQuery, orgParams);

                    string orgAddedMessage = $"Добавлена организация: {orgName}";
                    StressTestLog.AppendText(orgAddedMessage + "\n");
                    Logger.WriteLog(orgAddedMessage);

                    for (int j = 1; j <= 100; j++)
                    {
                        string empName = $"Имя {j}";
                        string empLastName = $"Фамилия {j}";
                        string empPosition = "Сотрудник";

                        string insertEmpQuery = "INSERT INTO Employees (OrganizationId, FirstName, LastName, Position) VALUES (@OrgId, @FirstName, @LastName, @Position)";
                        var empParams = new[]
                        {
                    new SqlParameter("@OrgId", organizationId),
                    new SqlParameter("@FirstName", empName),
                    new SqlParameter("@LastName", empLastName),
                    new SqlParameter("@Position", empPosition),
                };

                        await _database.ExecuteNonQueryAsync(insertEmpQuery, empParams);
                    }

                    string employeesAddedMessage = $"Добавлены 100 сотрудников для организации {orgName}";
                    StressTestLog.AppendText(employeesAddedMessage + "\n");
                    Logger.WriteLog(employeesAddedMessage);
                }
                catch (Exception ex)
                {
                    string errorMessage = $"Ошибка при добавлении организации {orgName}: {ex.Message}";
                    StressTestLog.AppendText(errorMessage + "\n");
                    Logger.WriteLog(errorMessage);
                }

                StressTestLog.ScrollToEnd();
            }

            string finishMessage = "Стресс-тест завершён.";
            StressTestLog.AppendText(finishMessage + "\n");
            Logger.WriteLog(finishMessage);
        }

        /// <summary>
        /// Обработчик события изменения текста в логе стресс-теста.
        /// </summary>
        private void StressTestLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Можно чет придумать на будущее
        }

        /// <summary>
        /// Обработчик события изменения текста в поле поиска организаций.
        /// </summary>
        private void TxtSearchOrganizations_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Можно чет придумать на будущее
        }
    }
}
