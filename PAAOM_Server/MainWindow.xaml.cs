using LiveCharts;
using LiveCharts.Wpf;
using PAAOM_Server.Models;
using PAAOM_Server.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
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
					var signalData = signals[i]; // Сохраняем данные сигнала

					var viewModel = new MicrophoneGraphViewModel
					{
						Title = $"Микрофон {i + 1}",
						Series = new SeriesCollection
				{
					new LineSeries
					{
						Values = new ChartValues<double>(signalData),
						LineSmoothness = 0
					}
				},
						SignalData = signalData // Передаем данные сигнала
					};

					MicrophonesContainer.Items.Add(viewModel);
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

	public class MicrophoneGraphViewModel : INotifyPropertyChanged
	{
		private double[] _signalData;

		public string Title { get; set; }
		public SeriesCollection Series { get; set; }
		public ICommand ShowSpectrumCommand { get; }

		public double[] SignalData
		{
			get => _signalData;
			set
			{
				_signalData = value;
				OnPropertyChanged();
			}
		}

		public MicrophoneGraphViewModel()
		{
			ShowSpectrumCommand = new RelayCommand(ShowSpectrum, CanShowSpectrum);
		}

		private void ShowSpectrum()
		{
			if (SignalData == null || SignalData.Length == 0) return;

			var spectrumWindow = new SpectrumWindow(SignalData, Title, SignalGenerator.OutputSampleRate)
			{
				Owner = Application.Current.MainWindow
			};
			spectrumWindow.Show();
		}

		private bool CanShowSpectrum()
		{
			return SignalData != null && SignalData.Length > 0;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}