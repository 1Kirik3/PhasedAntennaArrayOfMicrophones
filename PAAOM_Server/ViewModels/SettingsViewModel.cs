using PAAOM_Common.Models.Interfaces;
using PAAOM_Common.Network.Interfaces;
using PAAOM_Common.Network.Models;
using PAAOM_Server.Services;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PAAOM_Server.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly INetworkService _networkService;
        private ushort _currentPacketId = 1234;
        private bool _isConnectionVerified = false;

        public EnvironmentSettingsViewModel EnvironmentSettings { get; }
        public AudioSourceViewModel AudioSource { get; }
        public MicrophoneArrayViewModel MicrophoneArray { get; }
        public NetworkSettingsViewModel NetworkSettings { get; }

        public RelayCommand ApplySettingsCommand { get; }
        public RelayCommand StartServerCommand { get; }
        public RelayCommand StopServerCommand { get; }
        public RelayCommand TestConnectionCommand { get; }
        public RelayCommand SendDetectionReportCommand { get; }
        public RelayCommand SendAdcDataCommand { get; }

        private string _networkStatus = "Сервер остановлен";
        public string NetworkStatus
        {
            get => _networkStatus;
            set
            {
                _networkStatus = value;
                OnPropertyChanged();
            }
        }

        private Brush _networkStatusColor = Brushes.Red;
        public Brush NetworkStatusColor
        {
            get => _networkStatusColor;
            set
            {
                _networkStatusColor = value;
                OnPropertyChanged();
            }
        }

        public SettingsViewModel(
            IEnvironmentSettings envSettings,
            IAudioSource audioSource,
            IMicrophoneArray microphoneArray,
            INetworkService networkService)
        {
            _networkService = networkService;
            _networkService.PropertyChanged += OnNetworkServicePropertyChanged;

            EnvironmentSettings = new EnvironmentSettingsViewModel(envSettings);
            AudioSource = new AudioSourceViewModel(audioSource);
            MicrophoneArray = new MicrophoneArrayViewModel(microphoneArray);
            NetworkSettings = new NetworkSettingsViewModel();

            ApplySettingsCommand = new RelayCommand(ApplySettings);
            StartServerCommand = new RelayCommand(StartServer, CanStartServer);
            StopServerCommand = new RelayCommand(StopServer, CanStopServer);
            TestConnectionCommand = new RelayCommand(TestConnection, CanTestConnection);
            SendDetectionReportCommand = new RelayCommand(SendDetectionReport, CanSendData);
            SendAdcDataCommand = new RelayCommand(SendAdcData, CanSendData);

            LoadAvailableIPs();

            // Подписка на события сети
            _networkService.AvailabilityResponseReceived += OnAvailabilityResponseReceived;
            _networkService.DetectionReportReceived += OnDetectionReportReceived;
            _networkService.AdcDataReceived += OnAdcDataReceived;
        }

        private async void StartServer()
        {
            try
            {
                var localEndpoint = new IPEndPoint(
                    IPAddress.Parse(NetworkSettings.LocalIP),
                    int.Parse(NetworkSettings.LocalPort));

                var remoteEndpoint = new IPEndPoint(
                    IPAddress.Parse(NetworkSettings.RemoteIP),
                    int.Parse(NetworkSettings.RemotePort));

                _networkService.Configure(localEndpoint, remoteEndpoint);
                _networkService.StartListening();

                NetworkStatus = "Сервер запущен";
                NetworkStatusColor = Brushes.Green;

                MessageBox.Show($"Сервер запущен на {localEndpoint}", "Успех");

                // Сбрасываем статус соединения
                _isConnectionVerified = false;

                // ВАЖНО: Обновляем состояние ВСЕХ команд
                UpdateAllCommands();
            }
            catch (Exception ex)
            {
                NetworkStatus = "Ошибка запуска";
                NetworkStatusColor = Brushes.Red;
                MessageBox.Show($"Ошибка при запуске сервера: {ex.Message}", "Ошибка");
            }
        }

        private void UpdateAllCommands()
        {
            CommandManager.InvalidateRequerySuggested();

            StartServerCommand.RaiseCanExecuteChanged();
            StopServerCommand.RaiseCanExecuteChanged();
            TestConnectionCommand.RaiseCanExecuteChanged();
            SendDetectionReportCommand.RaiseCanExecuteChanged();
            SendAdcDataCommand.RaiseCanExecuteChanged();
        }

        private async void TestConnection()
        {
            try
            {
                MessageBox.Show("Отправка запроса доступности...", "Проверка связи");

                bool isAvailable = await _networkService.CheckAvailabilityAsync(1234, 3000);

                _isConnectionVerified = isAvailable;

                UpdateAllCommands();

                MessageBox.Show(isAvailable
                    ? "Проверка связи успешна! Соединение установлено."
                    : "Нет ответа от антенны. Проверьте подключение.",
                    isAvailable ? "Успех" : "Ошибка");
            }
            catch (Exception ex)
            {
                _isConnectionVerified = false;
                UpdateAllCommands();

                MessageBox.Show($"Ошибка проверки связи: {ex.Message}", "Ошибка");
            }
        }

        private async void SendDetectionReport()
        {
            if (!_isConnectionVerified)
            {
                MessageBox.Show("Сначала выполните проверку связи!", "Предупреждение");
                return;
            }

            try
            {
                var detectionReport = new DetectionReport
                {
                    PacketId = _currentPacketId++,
                    DetectionTimeUnix = (uint)DateTimeOffset.Now.ToUnixTimeSeconds(),
                    DetectionTimeFine = 5000, // пример значения
                    MeasurementNumber = 1,
                    TargetType = 1, // пример: воздушная цель
                    Snr = 250, // ОСП
                    Bearing = 45.5f, // азимут 45.5 градусов
                    BearingRate = 2.1f, // ВИП
                    Distance = 1200, // дистанция 1200 м
                    DistanceRate = -5.3f, // ВИФ
                    AngleStdDev = 15, // СКО угла (1.5 градуса)
                    TimeStdDev = 25 // СКО времени (25 см)
                };

                bool success = await _networkService.SendAsync(detectionReport);
                MessageBox.Show(success ? "Отчет об обнаружении отправлен" : "Ошибка отправки",
                    success ? "Успех" : "Ошибка");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки отчета: {ex.Message}", "Ошибка");
            }
        }

        private async void SendAdcData()
        {
            if (!_isConnectionVerified)
            {
                MessageBox.Show("Сначала выполните проверку связи!", "Предупреждение");
                return;
            }

            try
            {
                var adcPacket = new AdcDataPacket
                {
                    PacketId = _currentPacketId++,
                    SequenceNumber = 1,
                    StartTime = (uint)(DateTime.Now.TimeOfDay.TotalSeconds)
                };

                // Заполняем тестовыми данными (синусоида разной частоты для каждого канала)
                for (int channel = 0; channel < 8; channel++)
                {
                    for (int i = 0; i < 125; i++)
                    {
                        double time = i / 125.0 * 2 * Math.PI;
                        double frequency = 1.0 + channel * 0.5; // Разная частота для каждого канала
                        adcPacket.ChannelSamples[channel][i] = (short)(Math.Sin(time * frequency) * short.MaxValue * 0.8);
                    }
                }

                bool success = await _networkService.SendAsync(adcPacket);
                MessageBox.Show(success ? "ADC данные отправлены" : "Ошибка отправки",
                    success ? "Успех" : "Ошибка");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки ADC данных: {ex.Message}", "Ошибка");
            }
        }

        private void OnAvailabilityResponseReceived(object sender, AvailabilityResponse response)
        {
            MessageBox.Show($"Получен ответ доступности: PacketId={response.PacketId}", "Ответ");
        }

        private void OnDetectionReportReceived(object sender, DetectionReport report)
        {
            MessageBox.Show($"Получен отчет об обнаружении: Азимут={report.Bearing}°, Дистанция={report.Distance}м", "Отчет");
        }

        private void OnAdcDataReceived(object sender, AdcDataPacket data)
        {
            MessageBox.Show($"Получены ADC данные: {data.SequenceNumber}, {data.ChannelSamples[0].Length} отсчетов", "ADC Данные");
        }

        private bool CanStartServer() => !_networkService.IsListening;
        private bool CanStopServer() => _networkService.IsListening;
        private bool CanTestConnection()
        {
            bool canExecute = _networkService.IsListening;
            Console.WriteLine($"CanTestConnection: {canExecute}, IsListening: {_networkService.IsListening}");
            return canExecute;
        }
        private bool CanSendData() => _isConnectionVerified && _networkService.IsListening;


        private void LoadAvailableIPs()
        {
            NetworkSettings.AvailableIPs.Clear();
            NetworkSettings.AvailableIPs.Add("127.0.0.1");

            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork))
                {
                    NetworkSettings.AvailableIPs.Add(ip.ToString());
                }
            }
            catch (Exception ex)
            {
            }

            if (NetworkSettings.AvailableIPs.Count > 0)
            {
                NetworkSettings.LocalIP = NetworkSettings.AvailableIPs[0];
            }
        }


        private void StopServer()
        {
            try
            {
                _networkService.StopListening();
                NetworkStatus = "Сервер остановлен";
                NetworkStatusColor = Brushes.Red;

                _isConnectionVerified = false;

                UpdateAllCommands();

                MessageBox.Show("Сервер остановлен", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при остановке сервера: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplySettings()
        {
            MicrophoneArray.UpdateGeometry();
            //_logger?.LogInformation("Настройки применены");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnNetworkServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(INetworkService.IsListening))
            {
                UpdateAllCommands();
            }
        }
    }
}