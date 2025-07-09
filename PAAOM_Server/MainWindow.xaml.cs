using PAAOM_Server.Models;
using PAAOM_Server.ViewModels;
using System.Windows;
using System.Windows.Media.Media3D;

namespace PAAOM_Server
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private EnvironmentSettings _environmentSettings;
		private AudioSource _audioSource;
		private MicrophoneArray _array;

		private EnvironmentSettingsViewModel _environmentSettingsViewModel;
		private AudioSourceViewModel _audioSourceViewModel;
		private MicrophoneArrayViewModel _arrayViewModel;
		private MainViewModel _mainViewModel;

		public MainWindow()
		{
			InitializeComponent();
			InitilizeModels();
			InitilizeViewModels();
		}

		private void SettingsButton_Click(object sender, RoutedEventArgs e)
		{
			if (_environmentSettings == null || _audioSource == null || _array == null)
			{
				MessageBox.Show("Ошибка инициализации параметров", "Ошибка",
							  MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			var settingsWindow = new SettingsWindow
			{
				Owner = this,
				DataContext = new SettingsViewModel(
					_environmentSettings,
					_audioSource,
					_array)
			};

			if (settingsWindow.ShowDialog() == true)
			{
				_array.UpdateGeometry();
			}
		}

		private void InitilizeViewModels()
		{
			var envViewModel = new EnvironmentSettingsViewModel(_environmentSettings);
			var audioSourceViewModel = new AudioSourceViewModel(_audioSource);
			var array = new MicrophoneArrayViewModel(_array);

			_mainViewModel = new MainViewModel(_environmentSettings, _audioSource, _array);

			this.DataContext = _mainViewModel;
		}

		private void InitilizeModels()
		{
			_environmentSettings = new EnvironmentSettings
			{
				TemperatureCelsius = 20.0f,
				NoiseLevel = 0.05f
			};

			_audioSource = new AudioSource
			{
				Frequency = 500f,
				Amplitude = 1.0f,
				Position = new Point3D(5, 5, 0)
			};

			_array = new MicrophoneArray(_environmentSettings, _audioSource)
			{
				Radius = 0.5f,
				ArrayCenter = new Point3D(0, 0, 0)
			};
		}

	}
}