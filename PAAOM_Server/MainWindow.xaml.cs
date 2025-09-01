using LiveCharts;
using LiveCharts.Wpf;
using PAAOM_Common.Models;
using PAAOM_Common.Network.Interfaces;
using PAAOM_Common.Network.Services;
using PAAOM_Server.Services;
using PAAOM_Server.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace PAAOM_Server
{
    public partial class MainWindow : Window
    {
        private EnvironmentSettings _environmentSettings;
        private AudioSource _audioSource;
        private MicrophoneArray _array;
        private SignalGenerator _signalGenerator;
        private INetworkService _networkService;

        private EnvironmentSettingsViewModel _environmentSettingsViewModel;
        private AudioSourceViewModel _audioSourceViewModel;
        private MicrophoneArrayViewModel _arrayViewModel;
        private MainViewModel _mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            InitializeModels();
            InitializeNetworkService();
            InitializeViewModels();
        }

        private void InitializeNetworkService()
        {
            var crcCalculator = new Crc16Calculator();
            var packetBuilder = new PacketBuilder(crcCalculator);
            _networkService = new UdpNetworkService(packetBuilder);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_environmentSettings == null || _audioSource == null || _array == null || _networkService == null)
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
                    _array,
                    _networkService)
            };

            if (settingsWindow.ShowDialog() == true)
            {
                _array.UpdateGeometry();
            }
        }

        private void InitializeViewModels()
        {
            _mainViewModel = new MainViewModel(_environmentSettings, _audioSource, _array);
            this.DataContext = _mainViewModel;
        }

        private void InitializeModels()
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

                if (signals.Count != microphoneCount) return;

                for (int i = 0; i < microphoneCount; i++)
                {
                    var signalData = signals[i];

                    var viewModel = new MicrophoneGraphViewModel(_audioSource.Frequency)
                    {
                        Title = $"Микрофон {i + 1}",
                        SignalData = signalData, 
                        Series = new SeriesCollection
                {
                    new LineSeries
                    {
                        Values = new ChartValues<double>(signalData),
                        LineSmoothness = 0,
                        PointGeometry = null
                    }
                }
                    };

                    MicrophonesContainer.Items.Add(viewModel);
                }
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
        private readonly double _signalFrequency;
        private double _minAmplitude;
        private double _maxAmplitude;

        public double TimeStep { get; } = 5.0;

        public Func<double, string> XAxisFormatter => value =>
            (value % TimeStep == 0) ? $"{value:F0}" : string.Empty;

        public Func<double, string> YAxisFormatter { get; } = value => $"{value:F2}";

        public string Title { get; set; }
        public SeriesCollection Series { get; set; }
        public ICommand ShowSpectrumCommand { get; }

        public double MinAmplitude
        {
            get => _minAmplitude;
            set
            {
                _minAmplitude = value;
                OnPropertyChanged();
            }
        }

        public double MaxAmplitude
        {
            get => _maxAmplitude;
            set
            {
                _maxAmplitude = value;
                OnPropertyChanged();
            }
        }

        public double[] SignalData
        {
            get => _signalData;
            set
            {
                _signalData = value;
                CalculateAmplitudeRange();
                OnPropertyChanged();
            }
        }

        public MicrophoneGraphViewModel(double signalFrequency)
        {
            _signalFrequency = signalFrequency;
            ShowSpectrumCommand = new RelayCommand(ShowSpectrum, CanShowSpectrum);
        }

        private void CalculateAmplitudeRange()
        {
            if (SignalData == null || SignalData.Length == 0)
            {
                MinAmplitude = -1;
                MaxAmplitude = 1;
                return;
            }

            double max = SignalData.Max();
            double min = SignalData.Min();

            double margin = Math.Max(Math.Abs(max), Math.Abs(min)) * 0.1;
            MaxAmplitude = max + margin;
            MinAmplitude = min - margin;
        }

        private void ShowSpectrum()
        {
            if (SignalData == null || SignalData.Length == 0) return;

            var spectrumWindow = new SpectrumWindow(
                SignalData,
                Title,
                SignalGenerator.OutputSampleRate,
                _signalFrequency)
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