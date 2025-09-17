using PAAOM_Common.Models;
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
        private readonly ISettingsService _settingsService;
        private readonly IEnvironment _environment;
        private readonly IAudioSource _source;
        private readonly IMicrophoneArray _array;

        private readonly DetectionCalculator _detectionCalculator;
        private readonly AdcDataGenerator _adcDataGenerator;

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

        public ICommand LoadSettingsCommand { get; }
        public ICommand SaveSettingsCommand { get; }


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
            IEnvironment envSettings,
            IAudioSource audioSource,
            IMicrophoneArray microphoneArray,
            INetworkService networkService,
            ISettingsService settingsService)
        {
            _environment = envSettings;
            _source = audioSource;
            _array = microphoneArray;
            _networkService = networkService;
            _settingsService = settingsService;

            _detectionCalculator = new DetectionCalculator(envSettings, audioSource, microphoneArray);
            _adcDataGenerator = new AdcDataGenerator(envSettings, audioSource, microphoneArray);

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

            LoadSettingsCommand = new RelayCommand(async () => await LoadSettingsAsync(true));
            SaveSettingsCommand = new RelayCommand(async () => await SaveSettingsAsync(true));
            _ = LoadSettingsAsync(false);

            LoadAvailableIPs();

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

                _isConnectionVerified = false;

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
                var detectionReport = _detectionCalculator.CalculateDetectionReport();
                detectionReport.PacketId = _currentPacketId++;

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
                var adcPacket = _adcDataGenerator.GenerateAdcData();
                adcPacket.PacketId = _currentPacketId++;

                // Проверяем данные перед отправкой
                ValidateAdcData(adcPacket);

                bool success = await _networkService.SendAsync(adcPacket);
                MessageBox.Show(success ? "ADC данные отправлены" : "Ошибка отправки",
                    success ? "Успех" : "Ошибка");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации ADC данных: {ex.Message}", "Ошибка");
            }
        }

        private void ValidateAdcData(AdcDataPacket packet)
        {
            for (int channel = 0; channel < 8; channel++)
            {
                if (packet.ChannelSamples[channel].Length != 125)
                {
                    throw new InvalidOperationException(
                        $"Канал {channel} содержит {packet.ChannelSamples[channel].Length} отсчетов вместо 125");
                }
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

        private async Task LoadSettingsAsync(bool showMessage = false)
        {
            try
            {
                var settings = await _settingsService.LoadSettingsAsync();

                await _settingsService.ApplySettingsAsync(settings, _environment, _source, _array,
                    () => {
                        NetworkSettings.LocalIP = settings.Network.LocalIP;
                        NetworkSettings.LocalPort = settings.Network.LocalPort;
                        NetworkSettings.RemoteIP = settings.Network.RemoteIP;
                        NetworkSettings.RemotePort = settings.Network.RemotePort;

                        OnPropertyChanged(nameof(NetworkSettings));
                    });

                //EnvironmentSettings.RefreshAllProperties();
                AudioSource.RefreshAllProperties();
                //MicrophoneArray.RefreshAllProperties();

                if (showMessage)
                {
                    MessageBox.Show("Настройки успешно загружены!", "Успех");
                }
            }
            catch (Exception ex)
            {
                if (showMessage)
                {
                    MessageBox.Show($"Ошибка загрузки настроек: {ex.Message}", "Ошибка");
                }
            }
        }

        private async Task SaveSettingsAsync(bool showMessage = false)
        {
            try
            {
                var settings = _settingsService.CreateSettingsFromModels(
                    _environment, _source, _array, NetworkSettings);

                await _settingsService.SaveSettingsAsync(settings);

                if (showMessage)
                {
                    MessageBox.Show("Настройки успешно сохранены!", "Успех");
                }
            }
            catch (Exception ex)
            {
                if (showMessage)
                {
                    MessageBox.Show($"Ошибка сохранения настроек: {ex.Message}", "Ошибка");
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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