using PAAOM_Server.ViewModels;
using System.Windows;


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
	}
}
