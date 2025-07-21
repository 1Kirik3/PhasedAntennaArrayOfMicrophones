using PAAOM_Server.ViewModels;
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

			// Автоматическое закрытие при успешном выполнении команды Apply
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

		private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			string currentText = textBox.Text;
			int cursorPos = textBox.CaretIndex;

			// Разрешаем:
			// - Цифры (0-9)
			// - Точку (.), но только одну и не в начале
			bool isDigit = char.IsDigit(e.Text, 0);
			bool isDot = e.Text == ".";

			// Блокируем все, кроме цифр и точки
			if (!isDigit && !isDot)
			{
				e.Handled = true;
				return;
			}

			// Особые правила для точки
			if (isDot)
			{
				// Запрещаем точку в начале
				if (cursorPos == 0)
				{
					e.Handled = true;
					return;
				}

				// Запрещаем вторую точку
				if (currentText.Contains("."))
				{
					e.Handled = true;
					return;
				}
			}
		}

		private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			// Разрешаем:
			// - Backspace, Delete
			// - Стрелки (влево/вправо)
			// - Tab
			if (e.Key == Key.Back || e.Key == Key.Delete ||
				e.Key == Key.Left || e.Key == Key.Right ||
				e.Key == Key.Tab)
			{
				return;
			}

			// Блокируем пробел
			if (e.Key == Key.Space)
			{
				e.Handled = true;
			}
		}
	}
}
