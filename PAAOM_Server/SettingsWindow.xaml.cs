using PAAOM_Server.ViewModels;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace PAAOM_Server
{
	/// <summary>
	/// Логика взаимодействия для SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		public SettingsWindow()
		{
			InitializeComponent();

			if (DataContext is SettingsViewModel vm)
			{
				vm.ApplySettingsCommand.CanExecuteChanged += (s, e) =>
				{
					if (vm.ApplySettingsCommand.CanExecute(null))
					{
						this.DialogResult = true;
						this.Close();
					}
				};
			}
		}

		private void ApplyButton_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Данные успешно обновлены!", "Обновление",
			  MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void NoiseLevelTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			var textBox = sender as TextBox;
			string currentText = textBox.Text;
			int caretIndex = textBox.CaretIndex;
			string newText = currentText.Substring(0, caretIndex) + e.Text + currentText.Substring(caretIndex);

			// Разрешаем только цифры, точку или запятую
			if (!(char.IsDigit(e.Text, 0) || e.Text == "." || e.Text == ","))
			{
				e.Handled = true;
				return;
			}

			// Проверяем, что в результате получится валидное число от 0 до 1
			if (decimal.TryParse(newText.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
			{
				if (result < 0 || result > 1)
				{
					e.Handled = true;
				}
			}
			else
			{
				// Разрешаем ввод точки/запятой, если её ещё нет
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
			if (string.IsNullOrEmpty(textBox.Text))
			{
				return;
			}

			int caretPosition = textBox.CaretIndex;

			// Проверяем, что текст содержит не более одной точки/запятой
			string text = textBox.Text;
			int dotCount = text.Count(c => c == '.');
			int commaCount = text.Count(c => c == ',');

			if (dotCount + commaCount > 1)
			{
				// Удаляем лишние точки/запятые
				bool hasDot = text.Contains('.');
				text = new string(text.Where(c => char.IsDigit(c) ||
												(c == '.' && !hasDot) ||
												(c == ',' && !hasDot)).ToArray());
				textBox.Text = text;
				textBox.CaretIndex = Math.Min(caretPosition, text.Length);
				return;
			}

			// Заменяем запятую на точку
			if (text.Contains(','))
			{
				text = text.Replace(',', '.');
				textBox.Text = text;
				textBox.CaretIndex = Math.Min(caretPosition, text.Length);
			}
		}
	}
}
