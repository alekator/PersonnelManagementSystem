using System;
using System.Windows;

namespace PersonnelManagementSystem
{
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

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
