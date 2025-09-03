using LiveCharts;
using LiveCharts.Wpf;
using PAAOM_Client.Services;
using PAAOM_Client.ViewModels;
using PAAOM_Common.Network.Interfaces;
using PAAOM_Common.Network.Models;
using PAAOM_Common.Network.Services;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Input;

namespace PAAOM_Client
{
    public partial class MainWindow : Window
    {
        private INetworkService _networkService;
        private MicrophoneGraphViewModel[] _channelViewModels;
        private DetectionInfoViewModel _detectionInfo;

        public MainWindow()
        {
            InitializeComponent();
            InitializeNetworkService();
            InitializeViewModels();
        }

        private void InitializeNetworkService()
        {
            var crcCalculator = new Crc16Calculator();
            var packetBuilder = new PacketBuilder(crcCalculator);
            _networkService = new UdpNetworkService(packetBuilder);

            _networkService.AvailabilityRequestReceived += OnAvailabilityRequestReceived;
            _networkService.AvailabilityResponseReceived += OnAvailabilityResponseReceived;
            _networkService.DetectionReportReceived += OnDetectionReportReceived;
            _networkService.AdcDataReceived += OnAdcDataReceived;
        }

        private void InitializeViewModels()
        {
            _detectionInfo = new DetectionInfoViewModel();
            DetectionPanel.DataContext = _detectionInfo;

            _channelViewModels = new MicrophoneGraphViewModel[8];
            for (int i = 0; i < 8; i++)
            {
                _channelViewModels[i] = new MicrophoneGraphViewModel
                {
                    Title = $"Канал {i + 1}",
                    ChannelIndex = i
                };
            }

            MicrophonesContainer.ItemsSource = _channelViewModels;
        }

        private async void OnAvailabilityRequestReceived(object sender, AvailabilityRequest request)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = $"Запрос проверки связи: #{request.PacketId}";
            });

            try
            {
                var response = new AvailabilityResponse { PacketId = request.PacketId };
                await _networkService.SendAsync(response);

                Dispatcher.Invoke(() =>
                {
                    StatusText.Text = $"Отправлен ответ на проверку связи: #{request.PacketId}";
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    StatusText.Text = $"Ошибка отправки ответа: {ex.Message}";
                });
            }
        }

        private void OnAvailabilityResponseReceived(object sender, AvailabilityResponse response)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = $"Ответ на проверку связи: #{response.PacketId}";
                MessageBox.Show($"Получен ответ на проверку связи от сервера. PacketId: {response.PacketId}",
                    "Проверка связи", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        private void OnDetectionReportReceived(object sender, DetectionReport report)
        {
            Dispatcher.Invoke(() =>
            {
                //StatusText.Text = $"Обнаружение: Азимут={report.Bearing}°, Дистанция={report.Distance}м";
                _detectionInfo.UpdateFromReport(report);
            });
        }

        private void OnAdcDataReceived(object sender, AdcDataPacket adcData)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = $"ADC данные: Пакет #{adcData.SequenceNumber}";

                for (int channel = 0; channel < 8; channel++)
                {
                    if (channel < adcData.ChannelSamples.Length)
                    {
                        double[] signalData = adcData.ChannelSamples[channel]
                            .Select(s => (double)s / short.MaxValue)
                            .ToArray();

                        _channelViewModels[channel].UpdateSignalData(signalData, adcData);
                    }
                }
            });
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var localEndpoint = new IPEndPoint(IPAddress.Any, 5001);
                var remoteEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000);

                _networkService.Configure(localEndpoint, remoteEndpoint);
                _networkService.StartListening();

                StatusText.Text = "Ожидание данных от сервера...";
                ConnectButton.IsEnabled = false;
                DisconnectButton.IsEnabled = true;

                MessageBox.Show("Клиент запущен. Ожидайте проверки связи от сервера.", "Готов",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка");
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            _networkService.StopListening();
            StatusText.Text = "Отключено";
            ConnectButton.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
        }
    }

    public class MicrophoneGraphViewModel : INotifyPropertyChanged
    {
        private double[] _signalData;
        private string _packetInfo;
        private double _minAmplitude;
        private double _maxAmplitude;

        public int ChannelIndex { get; set; }
        public string Title { get; set; }

        public double TimeStep { get; } = 5.0;

        public Func<double, string> XAxisFormatter => value =>
            (value % TimeStep == 0) ? $"{value:F0}" : string.Empty;

        public Func<double, string> YAxisFormatter { get; } = value => $"{value:F2}";

        public SeriesCollection Series { get; } = new SeriesCollection
        {
            new LineSeries
            {
                Values = new ChartValues<double>(),
                LineSmoothness = 0,
                PointGeometry = null
            }
        };

        public ICommand ShowSpectrumCommand { get; }

        public string PacketInfo
        {
            get => _packetInfo;
            set
            {
                _packetInfo = value;
                OnPropertyChanged();
            }
        }

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

        public MicrophoneGraphViewModel()
        {
            ShowSpectrumCommand = new RelayCommand(ShowSpectrum, CanShowSpectrum);
        }

        public void UpdateSignalData(double[] signalData, AdcDataPacket packet)
        {
            _signalData = signalData;

            Series[0].Values.Clear();
            foreach (var value in signalData)
            {
                Series[0].Values.Add(value);
            }

            PacketInfo = $"Пакет #{packet.SequenceNumber}, Время: {packet.StartTime}s";

            CalculateAmplitudeRange();

            OnPropertyChanged(nameof(Series));
        }

        private void CalculateAmplitudeRange()
        {
            if (_signalData == null || _signalData.Length == 0)
            {
                MinAmplitude = -1;
                MaxAmplitude = 1;
                return;
            }

            double max = _signalData.Max();
            double min = _signalData.Min();

            double margin = Math.Max(Math.Abs(max), Math.Abs(min)) * 0.1;
            MaxAmplitude = max + margin;
            MinAmplitude = min - margin;
        }

        private void ShowSpectrum()
        {
            if (_signalData == null || _signalData.Length == 0) return;

            MessageBox.Show($"Спектр для канала {ChannelIndex + 1}", "Спектр",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanShowSpectrum()
        {
            return _signalData != null && _signalData.Length > 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}