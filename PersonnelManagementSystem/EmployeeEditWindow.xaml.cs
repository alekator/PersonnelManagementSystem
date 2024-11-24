using System;
using System.Windows;

namespace PersonnelManagementSystem
{
    public partial class EmployeeEditWindow : Window
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Position { get; private set; }

        public EmployeeEditWindow(string firstName = "", string lastName = "", string position = "")
        {
            InitializeComponent();

            TxtFirstName.Text = firstName;
            TxtLastName.Text = lastName;
            TxtPosition.Text = position;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtFirstName.Text) ||
                string.IsNullOrWhiteSpace(TxtLastName.Text) ||
                string.IsNullOrWhiteSpace(TxtPosition.Text))
            {
                MessageBox.Show("Все поля должны быть заполнены.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            FirstName = TxtFirstName.Text.Trim();
            LastName = TxtLastName.Text.Trim();
            Position = TxtPosition.Text.Trim();

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
