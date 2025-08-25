using PAAOM_Common.Models;
using System.Windows;

namespace PAAOM_Server
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			var envSettings = new EnvironmentSettings();
			var audioSource = new AudioSource();
			var micArray = new MicrophoneArray(envSettings, audioSource);

			var mainWindow = new MainWindow();
			mainWindow.Show();
		}
	}

}
