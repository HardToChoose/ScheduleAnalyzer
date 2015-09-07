using System.Windows;

namespace MainApp.Views
{
    public partial class TaskGenerationDialog : Window
    {
        public TaskGenerationDialog()
        {
            InitializeComponent();
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
