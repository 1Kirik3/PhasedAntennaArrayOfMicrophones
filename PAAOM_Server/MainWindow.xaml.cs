using LiveCharts;
using LiveCharts.Wpf;
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
		private SignalGenerator _signalGenerator;

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

			_signalGenerator = new SignalGenerator();
		}

		private void UpdateButton_Click(object sender, RoutedEventArgs e)
		{
			UpdateGraphs();
			MessageBox.Show("Графики успешно обновлены!", "Обновление",
						  MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void UpdateGraphs()
		{
			try
			{
				int microphoneCount = _array.MicrophonesCount;
				var signals = _signalGenerator.GenerateSignals(_array, _audioSource, _environmentSettings);
				MicrophonesContainer.Items.Clear();

				if (signals.Count != microphoneCount)
				{
					MessageBox.Show($"Ошибка: получено {signals.Count} сигналов, но микрофонов {microphoneCount}",
								  "Несоответствие данных",
								  MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				for (int i = 0; i < microphoneCount; i++)
				{
					MicrophonesContainer.Items.Add(new MicrophoneGraphViewModel
					{
						Title = $"Микрофон {i + 1}",
						Series = new SeriesCollection
				{
					new LineSeries
					{
						Values = new ChartValues<double>(signals[i]),
						LineSmoothness = 0
					}
				}
					});
				}

				MicrophonesContainer.UpdateLayout();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка обновления: {ex.Message}", "Ошибка",
							  MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}

	public class MicrophoneGraphViewModel
	{
		public string Title { get; set; }
		public SeriesCollection Series { get; set; }
	}
}