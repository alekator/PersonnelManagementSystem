using System;
using System.Windows;

namespace PersonnelManagementSystem
{
    /// <summary>
    /// Окно для добавления или редактирования данных организации.
    /// </summary>
    public partial class OrganizationEditWindow : Window
    {
        public string OrganizationName { get; private set; }
        public string OrganizationAddress { get; private set; }
        public string OrganizationPhone { get; private set; }

        public OrganizationEditWindow(string name = "", string address = "", string phone = "")
        {
            InitializeComponent();

            TxtName.Text = name;
            TxtAddress.Text = address;
            TxtPhone.Text = phone;
        }


        /// <summary>
        /// Сохраняет данные, введённые пользователем, и закрывает окно.
        /// </summary>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text) ||
                string.IsNullOrWhiteSpace(TxtAddress.Text) ||
                string.IsNullOrWhiteSpace(TxtPhone.Text))
            {
                MessageBox.Show("Все поля должны быть заполнены.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OrganizationName = TxtName.Text.Trim();
            OrganizationAddress = TxtAddress.Text.Trim();
            OrganizationPhone = TxtPhone.Text.Trim();

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Закрывает окно без сохранения изменений.
        /// </summary>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
