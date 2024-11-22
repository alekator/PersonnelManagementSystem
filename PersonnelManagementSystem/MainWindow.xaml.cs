using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace PersonnelManagementSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Database _database;

        public MainWindow()
        {
            InitializeComponent();
            _database = new Database();
            this.Loaded += MainWindow_Load;
        }

        /// <summary>
        /// Событие загрузки главного окна.
        /// </summary>
        private void MainWindow_Load(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем подключение к базе данных
                DataTable organizations = _database.ExecuteSelectQuery("SELECT * FROM Organizations");

                Logger.WriteLog($"Успешное подключение к базе данных. Найдено организаций: {organizations.Rows.Count}");
                MessageBox.Show("Подключение к базе данных успешно!");
                LoadOrganizationsToUI(organizations);
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Ошибка подключения к базе данных: {ex.Message}");
                MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

    }
}
