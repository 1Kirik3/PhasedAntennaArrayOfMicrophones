using PAAOM_Server.ViewModels;
using System.Windows;

namespace PAAOM_Server
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ApplySettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                viewModel.ApplySettingsCommand.Execute(null);
                MessageBox.Show("Настройки успешно применены!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}