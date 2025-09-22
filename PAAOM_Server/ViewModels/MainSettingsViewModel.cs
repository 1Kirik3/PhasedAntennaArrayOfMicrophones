using PAAOM_Common;
using PAAOM_Common.Models.Interfaces;
using PAAOM_Common.Network.Interfaces;
using PAAOM_Common.Network.Models;
using PAAOM_Server.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace PAAOM_Server.ViewModels
{
    public class MainSettingsViewModel : INotifyPropertyChanged
    {
        private readonly ISettingsService _settingsService;
        private readonly IEnvironment _environment;
        private readonly IAudioSource _source;
        private readonly IMicrophoneArray _array;

        private readonly DetectionCalculator _detectionCalculator;
        private readonly AdcDataGenerator _adcDataGenerator;

        private ushort _currentPacketId = 1234;
        private uint _timeCounter = 0;

        public EnvironmentSettingsViewModel EnvironmentSettings { get; }
        public AudioSourceViewModel AudioSource { get; }
        public MicrophoneArrayViewModel MicrophoneArray { get; }
        public NetworkSettingsViewModel NetworkSettings { get; }

        public RelayCommand ApplySettingsCommand { get; }
        public RelayCommand SendDetectionReportCommand { get; }
        public RelayCommand SendAdcDataCommand { get; }

        public ICommand LoadSettingsCommand { get; }
        public ICommand SaveSettingsCommand { get; }

        private double _soundTransmissionTimeMs = 0;
        public double SoundTransmissionTimeMs
        {
            get => _soundTransmissionTimeMs;
            set
            {
                _soundTransmissionTimeMs = value;
                OnPropertyChanged();
            }
        }

        private double _currentDistance = 0;
        public double CurrentDistance
        {
            get => _currentDistance;
            set
            {
                _currentDistance = value;
                OnPropertyChanged();
            }
        }

        public MainSettingsViewModel(
            IEnvironment envSettings,
            IAudioSource audioSource,
            IMicrophoneArray microphoneArray,
            INetworkService networkService,
            ISettingsService settingsService)
        {
            _environment = envSettings;
            _source = audioSource;
            _array = microphoneArray;
            _settingsService = settingsService;

            _detectionCalculator = new DetectionCalculator(envSettings, audioSource, microphoneArray);
            _adcDataGenerator = new AdcDataGenerator(envSettings, audioSource, microphoneArray);

            NetworkSettings = new NetworkSettingsViewModel(networkService);
            EnvironmentSettings = new EnvironmentSettingsViewModel(envSettings);
            AudioSource = new AudioSourceViewModel(audioSource);
            MicrophoneArray = new MicrophoneArrayViewModel(microphoneArray);

            ApplySettingsCommand = new RelayCommand(ApplySettings);
            SendDetectionReportCommand = new RelayCommand(SendDetectionReport, CanSendData);
            SendAdcDataCommand = new RelayCommand(SendAdcData, CanSendData);

            LoadSettingsCommand = new RelayCommand(async () => await LoadSettingsAsync(true));
            SaveSettingsCommand = new RelayCommand(async () => await SaveSettingsAsync(true));
            _ = LoadSettingsAsync(false);

            CalculateSoundTransmissionTime();
            NetworkSettings.SetDataGenerators(GenerateDetectionReportForPeriodicSend, GenerateAdcDataForPeriodicSend, IncrementTimeCounter);

            // Подписываемся на изменения для пересчета времени передачи звука
            AudioSource.PropertyChanged += OnAudioSourcePropertyChanged;
            EnvironmentSettings.PropertyChanged += OnEnvironmentPropertyChanged;

            // Подписываемся на изменения сетевого статуса для обновления команд
            NetworkSettings.PropertyChanged += OnNetworkSettingsPropertyChanged;
        }

        ~MainSettingsViewModel()
        {
            // Отписываемся от событий
            if (AudioSource != null)
            {
                AudioSource.PropertyChanged -= OnAudioSourcePropertyChanged;
            }
            if (EnvironmentSettings != null)
            {
                EnvironmentSettings.PropertyChanged -= OnEnvironmentPropertyChanged;
            }
            if (NetworkSettings != null)
            {
                NetworkSettings.PropertyChanged -= OnNetworkSettingsPropertyChanged;
            }
        }

        private void OnAudioSourcePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AudioSource.X) ||
                e.PropertyName == nameof(AudioSource.Y) ||
                e.PropertyName == nameof(AudioSource.Z))
            {
                // Пересчитываем время передачи при изменении позиции источника
                CalculateSoundTransmissionTime();
            }
        }

        private void OnEnvironmentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EnvironmentSettings.SoundSpeed))
            {
                // Пересчитываем время передачи при изменении скорости звука
                CalculateSoundTransmissionTime();
            }
        }

        private void OnNetworkSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NetworkSettings.IsConnectionVerified) ||
                e.PropertyName == nameof(NetworkSettings.IsServerRunning))
            {
                // Обновляем доступность команд при изменении сетевого статуса
                UpdateCommands();
            }
        }

        private void CalculateSoundTransmissionTime()
        {
            try
            {
                var center = _array.ArrayCenter;
                var sourcePos = _source.Position;

                double dx = sourcePos.X - center.X;
                double dy = sourcePos.Y - center.Y;
                double dz = sourcePos.Z - center.Z;

                double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                CurrentDistance = Math.Round(distance, 2);

                double soundSpeed = _environment.SoundSpeed; // м/с
                SoundTransmissionTimeMs = (distance / soundSpeed) * 1000;

                // Обновляем интервал отправки в NetworkSettings
                NetworkSettings.SendingIntervalMs = Math.Max(10, SoundTransmissionTimeMs);

                Debug.WriteLine($"Расстояние: {distance:F2} м, Скорость звука: {soundSpeed:F2} м/с, " +
                               $"Время передачи: {SoundTransmissionTimeMs:F2} мс, Интервал: {NetworkSettings.SendingIntervalMs:F2} мс");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка расчета времени передачи: {ex.Message}");
                NetworkSettings.SendingIntervalMs = 100;
            }
        }

        private async void SendDetectionReport()
        {
            if (!NetworkSettings.IsConnectionVerified)
            {
                MessageBox.Show("Сначала выполните проверку связи!", "Предупреждение");
                return;
            }

            try
            {
                var detectionReport = _detectionCalculator.CalculateDetectionReport();
                detectionReport.PacketId = _currentPacketId++;

                bool success = await NetworkSettings.SendDetectionReportAsync(detectionReport);
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
            if (!NetworkSettings.IsConnectionVerified)
            {
                MessageBox.Show("Сначала выполните проверку связи!", "Предупреждение");
                return;
            }

            try
            {
                var adcPacket = _adcDataGenerator.GenerateAdcData();
                adcPacket.PacketId = _currentPacketId++;

                ValidateAdcData(adcPacket);

                bool success = await NetworkSettings.SendAdcDataAsync(adcPacket);
                MessageBox.Show(success ? "ADC данные отправлены" : "Ошибка отправки",
                    success ? "Успех" : "Ошибка");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации ADC данных: {ex.Message}", "Ошибка");
            }
        }

        public async Task<PacketBase> GenerateDetectionReportForPeriodicSend()
        {
            try
            {
                var detectionReport = _detectionCalculator.CalculateDetectionReport();
                detectionReport.PacketId = _currentPacketId++;
                return detectionReport;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка генерации отчета: {ex.Message}");
                return null;
            }
        }

        public async Task<PacketBase> GenerateAdcDataForPeriodicSend()
        {
            try
            {
                var adcPacket = _adcDataGenerator.GenerateAdcData();
                adcPacket.PacketId = _currentPacketId++;
                adcPacket.SequenceNumber = NetworkSettings.PacketCounter;
                adcPacket.StartTime = _timeCounter / Constants.SampleRate;

                ValidateAdcData(adcPacket);
                return adcPacket;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка генерации ADC данных: {ex.Message}");
                return null;
            }
        }

        public void IncrementTimeCounter()
        {
            _timeCounter += (uint)(NetworkSettings.SendingIntervalMs / 1000.0 * Constants.SampleRate);
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

        private bool CanSendData() => NetworkSettings.IsConnectionVerified && NetworkSettings.IsServerRunning;

        private void UpdateCommands()
        {
            SendDetectionReportCommand.RaiseCanExecuteChanged();
            SendAdcDataCommand.RaiseCanExecuteChanged();
        }

        private void ApplySettings()
        {
            MicrophoneArray.UpdateGeometry();
            CalculateSoundTransmissionTime();
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

                AudioSource.RefreshAllProperties();

                if (showMessage)
                {
                    MessageBox.Show("Настройки успешно загружены!", "Успех");
                }

                CalculateSoundTransmissionTime();
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
    }
}