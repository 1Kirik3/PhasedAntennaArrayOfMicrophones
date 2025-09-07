using PAAOM_Server.ViewModels;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PAAOM_Server
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NoiseLevelTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            string currentText = textBox.Text;
            int caretIndex = textBox.CaretIndex;
            string newText = currentText.Substring(0, caretIndex) + e.Text + currentText.Substring(caretIndex);

            if (!(char.IsDigit(e.Text, 0) || e.Text == "." || e.Text == ","))
            {
                e.Handled = true;
                return;
            }

            if (decimal.TryParse(newText.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                if (result < 0 || result > 1)
                {
                    e.Handled = true;
                }
            }
            else
            {
                if ((e.Text == "." || e.Text == ",") && !currentText.Contains('.') && !currentText.Contains(','))
                {
                    return;
                }
                e.Handled = true;
            }
        }

        private void NoiseLevelTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (string.IsNullOrEmpty(textBox.Text)) return;

            int caretPosition = textBox.CaretIndex;
            string text = textBox.Text;

            int dotCount = text.Count(c => c == '.');
            int commaCount = text.Count(c => c == ',');

            if (dotCount + commaCount > 1)
            {
                bool hasDot = text.Contains('.');
                text = new string(text.Where(c => char.IsDigit(c) ||
                                                (c == '.' && !hasDot) ||
                                                (c == ',' && !hasDot)).ToArray());
                textBox.Text = text;
                textBox.CaretIndex = Math.Min(caretPosition, text.Length);
                return;
            }

            if (text.Contains(','))
            {
                text = text.Replace(',', '.');
                textBox.Text = text;
                textBox.CaretIndex = Math.Min(caretPosition, text.Length);
            }
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