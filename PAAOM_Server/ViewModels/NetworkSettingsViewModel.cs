using PAAOM_Common.Network.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Net;
using System.Linq;
using System.Net.Sockets;
using System.Diagnostics;
using System;
using PAAOM_Server.Services;
using PAAOM_Common.Network.Models;

namespace PAAOM_Server.ViewModels
{
    public class NetworkSettingsViewModel : INotifyPropertyChanged
    {
        private readonly INetworkService _networkService;

        private string _localIP = "127.0.0.1";
        private string _localPort = "5000";
        private string _remoteIP = "127.0.0.1";
        private string _remotePort = "5001";
        private string _networkStatus = "Сервер остановлен";
        private Brush _networkStatusColor = Brushes.Red;
        private bool _isConnectionVerified = false;
        private bool _isSendingData = false;
        private double _sendingIntervalMs = 100;
        private ushort _packetCounter = 0;

        private Func<Task<PacketBase>> _detectionReportGenerator;
        private Func<Task<PacketBase>> _adcDataGenerator;
        private Action _incrementTimeCounter;

        public ObservableCollection<string> AvailableIPs { get; } = new ObservableCollection<string>();

        public string LocalIP
        {
            get => _localIP;
            set
            {
                _localIP = value;
                OnPropertyChanged();
            }
        }
        public string LocalPort
        {
            get => _localPort;
            set
            {
                _localPort = value;
                OnPropertyChanged();
            }
        }
        public string RemoteIP
        {
            get => _remoteIP;
            set
            {
                _remoteIP = value;
                OnPropertyChanged();
            }
        }
        public string RemotePort
        {
            get => _remotePort;
            set
            {
                _remotePort = value;
                OnPropertyChanged();
            }
        }
        public string NetworkStatus
        {
            get => _networkStatus;
            set
            {
                _networkStatus = value;
                OnPropertyChanged();
            }
        }
        public Brush NetworkStatusColor
        {
            get => _networkStatusColor;
            set
            {
                _networkStatusColor = value;
                OnPropertyChanged();
            }
        }
        public bool IsConnectionVerified
        {
            get => _isConnectionVerified;
            set
            {
                _isConnectionVerified = value;
                OnPropertyChanged();
                UpdateAllCommands();
            }
        }
        public bool IsSendingData
        {
            get => _isSendingData;
            set
            {
                _isSendingData = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSendingDataText));
            }
        }
        public string IsSendingDataText => IsSendingData ? "Отправка: ВКЛ" : "Отправка: ВЫКЛ";
        public double SendingIntervalMs
        {
            get => _sendingIntervalMs;
            set
            {
                _sendingIntervalMs = value;
                OnPropertyChanged();
            }
        }
        public ushort PacketCounter
        {
            get => _packetCounter;
            set
            {
                _packetCounter = value;
                OnPropertyChanged();
            }
        }
        public INetworkService NetworkService => _networkService;
        public bool IsServerRunning => _networkService.IsListening;

        public RelayCommand StartServerCommand { get; }
        public RelayCommand StopServerCommand { get; }
        public RelayCommand TestConnectionCommand { get; }
        public RelayCommand ToggleDataSendingCommand { get; }

        private System.Timers.Timer _dataSendTimer;
        private ushort _currentPacketId = 1234;

        public NetworkSettingsViewModel(INetworkService networkService)
        {
            _networkService = networkService;
            _networkService.PropertyChanged += OnNetworkServicePropertyChanged;

            StartServerCommand = new RelayCommand(StartServer, CanStartServer);
            StopServerCommand = new RelayCommand(StopServer, CanStopServer);
            TestConnectionCommand = new RelayCommand(TestConnection, CanTestConnection);
            ToggleDataSendingCommand = new RelayCommand(ToggleDataSending, CanToggleDataSending);

            _dataSendTimer = new System.Timers.Timer(SendingIntervalMs);
            _dataSendTimer.Elapsed += async (s, e) => await SendDataPeriodically();
            _dataSendTimer.AutoReset = true;

            _networkService.AvailabilityResponseReceived += OnAvailabilityResponseReceived;
            _networkService.DetectionReportReceived += OnDetectionReportReceived;
            _networkService.AdcDataReceived += OnAdcDataReceived;

            LoadAvailableIPs();
        }

        private void LoadAvailableIPs()
        {
            AvailableIPs.Clear();
            AvailableIPs.Add("127.0.0.1");

            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork))
                {
                    AvailableIPs.Add(ip.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка получения IP адресов: {ex.Message}");
            }

            if (AvailableIPs.Count > 0)
            {
                LocalIP = AvailableIPs[0];
            }
        }

        private void StartServer()
        {
            try
            {
                var localEndpoint = new IPEndPoint(
                    IPAddress.Parse(LocalIP),
                    int.Parse(LocalPort));

                var remoteEndpoint = new IPEndPoint(
                    IPAddress.Parse(RemoteIP),
                    int.Parse(RemotePort));

                _networkService.Configure(localEndpoint, remoteEndpoint);
                _networkService.StartListening();

                NetworkStatus = "Сервер запущен";
                NetworkStatusColor = Brushes.Green;

                MessageBox.Show($"Сервер запущен на {localEndpoint}", "Успех");

                IsConnectionVerified = false;
                UpdateAllCommands();
            }
            catch (Exception ex)
            {
                NetworkStatus = "Ошибка запуска";
                NetworkStatusColor = Brushes.Red;
                MessageBox.Show($"Ошибка при запуске сервера: {ex.Message}", "Ошибка");
            }
        }

        private void StopServer()
        {
            try
            {
                _networkService.StopListening();
                NetworkStatus = "Сервер остановлен";
                NetworkStatusColor = Brushes.Red;

                IsConnectionVerified = false;
                IsSendingData = false;
                _dataSendTimer.Stop();

                UpdateAllCommands();

                MessageBox.Show("Сервер остановлен", "Информация");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при остановке сервера: {ex.Message}", "Ошибка");
            }
        }

        private async void TestConnection()
        {
            try
            {
                MessageBox.Show("Отправка запроса доступности...", "Проверка связи");

                bool isAvailable = await _networkService.CheckAvailabilityAsync(1234, 3000);

                IsConnectionVerified = isAvailable;

                MessageBox.Show(isAvailable
                    ? "Проверка связи успешна! Соединение установлено."
                    : "Нет ответа от антенны. Проверьте подключение.",
                    isAvailable ? "Успех" : "Ошибка");
            }
            catch (Exception ex)
            {
                IsConnectionVerified = false;
                MessageBox.Show($"Ошибка проверки связи: {ex.Message}", "Ошибка");
            }
        }

        private void ToggleDataSending()
        {
            IsSendingData = !IsSendingData;

            if (IsSendingData)
            {
                _dataSendTimer.Start();
                Debug.WriteLine("Периодическая отправка данных запущена");
            }
            else
            {
                _dataSendTimer.Stop();
                Debug.WriteLine("Периодическая отправка данных остановлена");
            }

            UpdateAllCommands();
        }

        private async Task SendDataPeriodically()
        {
            if (!IsSendingData || !_networkService.IsListening) return;

            try
            {
                _incrementTimeCounter?.Invoke();

                if (_adcDataGenerator != null)
                {
                    var adcData = await _adcDataGenerator();
                    if (adcData != null)
                    {
                        await _networkService.SendAsync(adcData);
                    }
                }

                if (_packetCounter % 10 == 0 && _detectionReportGenerator != null)
                {
                    var detectionReport = await _detectionReportGenerator();
                    if (detectionReport != null)
                    {
                        await _networkService.SendAsync(detectionReport);
                    }
                }

                PacketCounter = _packetCounter;
                _packetCounter++;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка отправки данных: {ex.Message}");
            }
        }

        public async Task<bool> SendDetectionReportAsync(PacketBase detectionReport)
        {
            if (!IsConnectionVerified) return false;

            try
            {
                return await _networkService.SendAsync(detectionReport);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка отправки отчета: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendAdcDataAsync(PacketBase adcData)
        {
            if (!IsConnectionVerified) return false;

            try
            {
                return await _networkService.SendAsync(adcData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка отправки ADC данных: {ex.Message}");
                return false;
            }
        }

        public void SetDataGenerators(Func<Task<PacketBase>> detectionReportGenerator,
                             Func<Task<PacketBase>> adcDataGenerator,
                             Action incrementTimeCounter)
        {
            _detectionReportGenerator = detectionReportGenerator;
            _adcDataGenerator = adcDataGenerator;
            _incrementTimeCounter = incrementTimeCounter;
        }

        private void OnAvailabilityResponseReceived(object sender, object response)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Получен ответ доступности", "Ответ");
            });
        }

        private void OnDetectionReportReceived(object sender, object report)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Получен отчет об обнаружении", "Отчет");
            });
        }

        private void OnAdcDataReceived(object sender, object data)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Получены ADC данные", "ADC Данные");
            });
        }

        private bool CanStartServer() => !_networkService.IsListening;
        private bool CanStopServer() => _networkService.IsListening;
        private bool CanTestConnection() => _networkService.IsListening;
        private bool CanToggleDataSending() => IsConnectionVerified && _networkService.IsListening;

        private void UpdateAllCommands()
        {
            StartServerCommand.RaiseCanExecuteChanged();
            StopServerCommand.RaiseCanExecuteChanged();
            TestConnectionCommand.RaiseCanExecuteChanged();
            ToggleDataSendingCommand.RaiseCanExecuteChanged();
        }

        private void OnNetworkServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(INetworkService.IsListening))
            {
                OnPropertyChanged(nameof(IsServerRunning));
                UpdateAllCommands();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            _dataSendTimer?.Stop();
            _dataSendTimer?.Dispose();

            _networkService.PropertyChanged -= OnNetworkServicePropertyChanged;
            _networkService.AvailabilityResponseReceived -= OnAvailabilityResponseReceived;
            _networkService.DetectionReportReceived -= OnDetectionReportReceived;
            _networkService.AdcDataReceived -= OnAdcDataReceived;
        }
    }
}