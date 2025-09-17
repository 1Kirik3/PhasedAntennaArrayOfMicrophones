// MainViewModel.cs
using PAAOM_Common.Models;
using PAAOM_Common.Models.Interfaces;
using PAAOM_Server.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace PAAOM_Server.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public EnvironmentSettingsViewModel EnvironmentSettingsVM { get; set; }
        public AudioSourceViewModel AudioSourceVM { get; set; }
        public MicrophoneArrayViewModel MicrophoneArrayVM { get; set; }

        private readonly SignalGenerator _signalGenerator;
        private readonly ISettingsService _settingsService;
        private readonly IMicrophoneArray _array;
        private readonly IAudioSource _source;
        private readonly IEnvironment _environment;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand LoadSettingsCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand UpdateSignalsCommand { get; }

        private ObservableCollection<MicrophoneSignalViewModel> _microphoneSignals { get; }

        public MainViewModel(
            IEnvironment envSettings,
            IAudioSource audioSource,
            IMicrophoneArray microphoneArray,
            ISettingsService settingsService)
        {
            if (envSettings == null) throw new ArgumentNullException(nameof(envSettings));
            if (audioSource == null) throw new ArgumentNullException(nameof(audioSource));
            if (microphoneArray == null) throw new ArgumentNullException(nameof(microphoneArray));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _environment = envSettings;
            _source = audioSource;
            _array = microphoneArray;
            _settingsService = settingsService;
            _signalGenerator = new SignalGenerator();

            EnvironmentSettingsVM = new EnvironmentSettingsViewModel(envSettings);
            AudioSourceVM = new AudioSourceViewModel(audioSource);
            MicrophoneArrayVM = new MicrophoneArrayViewModel(microphoneArray);

            _microphoneSignals = new ObservableCollection<MicrophoneSignalViewModel>();

            UpdateSignalsCommand = new RelayCommand(UpdateSignals);
            LoadSettingsCommand = new RelayCommand(async () => await LoadSettingsAsync(true));
            SaveSettingsCommand = new RelayCommand(async () => await SaveSettingsAsync(true));

            LoadSettingsOnStartup();
        }

        public void UpdateSignals()
        {
            try
            {
                var signals = _signalGenerator.GenerateSignals(_array, _source, _environment);

                _microphoneSignals.Clear();

                for (int i = 0; i < signals.Count; i++)
                {
                    var signalVm = new MicrophoneSignalViewModel(i, signals[i]);
                    _microphoneSignals.Add(signalVm);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка обновления: {ex.Message}");
                MessageBox.Show($"UpdateSignals error: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadSettingsOnStartup()
        {
            try
            {
                var settings = await _settingsService.LoadSettingsAsync();
                await _settingsService.ApplySettingsAsync(settings, _environment, _source, _array);

                EnvironmentSettingsVM = new EnvironmentSettingsViewModel(_environment);
                AudioSourceVM = new AudioSourceViewModel(_source);
                MicrophoneArrayVM = new MicrophoneArrayViewModel(_array);

                OnPropertyChanged(nameof(EnvironmentSettingsVM));
                OnPropertyChanged(nameof(AudioSourceVM));
                OnPropertyChanged(nameof(MicrophoneArrayVM));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка загрузки настроек при старте: {ex.Message}");
            }
        }

        private async Task LoadSettingsAsync(bool showMessage = false)
        {
            try
            {
                var settings = await _settingsService.LoadSettingsAsync();
                await _settingsService.ApplySettingsAsync(settings, _environment, _source, _array);

                EnvironmentSettingsVM = new EnvironmentSettingsViewModel(_environment);
                AudioSourceVM = new AudioSourceViewModel(_source);
                MicrophoneArrayVM = new MicrophoneArrayViewModel(_array);

                OnPropertyChanged(nameof(EnvironmentSettingsVM));
                OnPropertyChanged(nameof(AudioSourceVM));
                OnPropertyChanged(nameof(MicrophoneArrayVM));

                if (showMessage)
                {
                    MessageBox.Show("Настройки успешно загружены!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                if (showMessage)
                {
                    MessageBox.Show($"Ошибка загрузки настроек: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task SaveSettingsAsync(bool showMessage = false)
        {
            try
            {
                var settings = _settingsService.CreateSettingsFromModels(_environment, _source, _array);
                await _settingsService.SaveSettingsAsync(settings);

                if (showMessage)
                {
                    MessageBox.Show("Настройки успешно сохранены!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                if (showMessage)
                {
                    MessageBox.Show($"Ошибка сохранения настроек: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}