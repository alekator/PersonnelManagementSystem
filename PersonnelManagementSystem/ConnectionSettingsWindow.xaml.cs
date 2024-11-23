using System;
using System.Windows;

namespace PersonnelManagementSystem
{
    public partial class ConnectionSettingsWindow : Window
    {
        public ConnectionSettingsWindow()
        {
            InitializeComponent();
            CmbAuthentication.SelectionChanged += CmbAuthentication_SelectionChanged;
            // Загружаем последние введённые данные
            TxtServer.Text = Properties.Settings.Default.LastServer;
            TxtDatabase.Text = Properties.Settings.Default.LastDatabase;
        }

        private void CmbAuthentication_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Показать/скрыть поля для ввода логина и пароля при выборе SQL Server Authentication
            if (CmbAuthentication.SelectedIndex == 1) // SQL Server Authentication
            {
                StackCredentials.Visibility = Visibility.Visible;
            }
            else
            {
                StackCredentials.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.WriteLog("BtnSave_Click: Начало сохранения настроек подключения.");

                string server = TxtServer.Text.Trim();
                string database = TxtDatabase.Text.Trim();

                if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(database))
                {
                    Logger.WriteLog("BtnSave_Click: Обнаружены пустые обязательные поля (Сервер или База данных).");
                    MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string connectionString;

                if (CmbAuthentication.SelectedIndex == 0) // Windows Authentication
                {
                    connectionString = $"Data Source={server};Initial Catalog={database};Integrated Security=True;Connection Timeout=60;";
                    Logger.WriteLog($"BtnSave_Click: Используется Windows Authentication. Строка подключения: {connectionString}");
                }
                else // SQL Server Authentication
                {
                    string username = TxtUsername.Text.Trim();
                    string password = TxtPassword.Password;

                    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    {
                        Logger.WriteLog("BtnSave_Click: Обнаружены пустые поля логина или пароля для SQL Server Authentication.");
                        MessageBox.Show("Введите имя пользователя и пароль для SQL Server Authentication.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    connectionString = $"Data Source={server};Initial Catalog={database};User ID={username};Password={password};Connection Timeout=60;";
                    Logger.WriteLog($"BtnSave_Click: Используется SQL Server Authentication. Строка подключения: {connectionString}");
                }

                // Сохраняем строку подключения
                DatabaseSettings.ConnectionString = connectionString;
                Logger.WriteLog("BtnSave_Click: Строка подключения успешно сохранена.");

                // Сохраняем последние введённые сервер и базу данных
                Properties.Settings.Default.LastServer = server;
                Properties.Settings.Default.LastDatabase = database;
                Properties.Settings.Default.Save();

                MessageBox.Show("Настройки подключения успешно сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"BtnSave_Click: Ошибка при настройке подключения. Подробности: {ex.Message}");
                MessageBox.Show($"Ошибка при настройке подключения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Logger.WriteLog("BtnSave_Click: Завершение метода.");
            }
        }
    }

    // Глобальный класс для хранения строки подключения
    public static class DatabaseSettings
    {
        public static string ConnectionString { get; set; }
    }
}
