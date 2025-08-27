using PAAOM_Common.Models.Interfaces;
using PAAOM_Common.Network.Interfaces;
using PAAOM_Common.Network.Models;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace PAAOM_Server.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly INetworkService _networkService;

        public EnvironmentSettingsViewModel EnvironmentSettings { get; }
        public AudioSourceViewModel AudioSource { get; }
        public MicrophoneArrayViewModel MicrophoneArray { get; }
        public NetworkSettingsViewModel NetworkSettings { get; }

        public RelayCommand ApplySettingsCommand { get; }
        public RelayCommand StartServerCommand { get; }
        public RelayCommand StopServerCommand { get; }

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

            EnvironmentSettings = new EnvironmentSettingsViewModel(envSettings);
            AudioSource = new AudioSourceViewModel(audioSource);
            MicrophoneArray = new MicrophoneArrayViewModel(microphoneArray);
            NetworkSettings = new NetworkSettingsViewModel();

            ApplySettingsCommand = new RelayCommand(ApplySettings);
            StartServerCommand = new RelayCommand(StartServer, CanStartServer);
            StopServerCommand = new RelayCommand(StopServer, CanStopServer);

            LoadAvailableIPs();

            _networkService.AvailabilityResponseReceived += OnAvailabilityResponseReceived;
            _networkService.DetectionReportReceived += OnDetectionReportReceived;
            _networkService.AdcDataReceived += OnAdcDataReceived;
        }

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

        private bool CanStartServer()
        {
            return !_networkService.IsListening &&
                   IPAddress.TryParse(NetworkSettings.LocalIP, out _) &&
                   int.TryParse(NetworkSettings.LocalPort, out int port) && port > 0 && port <= 65535 &&
                   IPAddress.TryParse(NetworkSettings.RemoteIP, out _) &&
                   int.TryParse(NetworkSettings.RemotePort, out port) && port > 0 && port <= 65535;
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

                MessageBox.Show($"Сервер запущен на {localEndpoint}", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                NetworkStatus = "Ошибка запуска";
                NetworkStatusColor = Brushes.Red;
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            StartServerCommand.RaiseCanExecuteChanged();
            StopServerCommand.RaiseCanExecuteChanged();
        }

        private bool CanStopServer()
        {
            return _networkService.IsListening;
        }

        private void StopServer()
        {
            try
            {
                _networkService.StopListening();
                NetworkStatus = "Сервер остановлен";
                NetworkStatusColor = Brushes.Red;
                MessageBox.Show("Сервер остановлен", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при остановке сервера: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            StartServerCommand.RaiseCanExecuteChanged();
            StopServerCommand.RaiseCanExecuteChanged();
        }

        private void OnAvailabilityResponseReceived(object sender, AvailabilityResponse response)
        {
            //_logger?.LogInformation("Получен ответ о доступности: PacketId={PacketId}", response.PacketId);
        }

        private void OnDetectionReportReceived(object sender, DetectionReport report)
        {
            //_logger?.LogInformation("Получен отчет об обнаружении");
        }

        private void OnAdcDataReceived(object sender, AdcDataPacket data)
        {
            //_logger?.LogDebug("Получены ADC данные");
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
    }
}