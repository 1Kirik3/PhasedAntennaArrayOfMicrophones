using PAAOM_Common.Models.Interfaces;
using PAAOM_Server.Services;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PAAOM_Server.ViewModels
{
    public class EnvironmentSettingsViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly IEnvironment _settings;
        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public EnvironmentSettingsViewModel(IEnvironment settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _settings.TemperatureChanged += OnTemperatureChanged;
            _settings.NoiseLevelChanged += OnNoiseLevelChanged;
        }

        ~EnvironmentSettingsViewModel()
        {
            _settings.TemperatureChanged -= OnTemperatureChanged;
            _settings.NoiseLevelChanged -= OnNoiseLevelChanged;
        }

        private void OnTemperatureChanged(float newValue)
        {
            OnPropertyChanged(nameof(Temperature));
            OnPropertyChanged(nameof(SoundSpeed));
            ClearErrors(nameof(Temperature));
        }

        private void OnNoiseLevelChanged(float newValue)
        {
            OnPropertyChanged(nameof(NoiseLevel));
            ClearErrors(nameof(NoiseLevel));
        }

        public bool HasErrors => _errors.Any();

        IEnumerable INotifyDataErrorInfo.GetErrors(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || !_errors.ContainsKey(propertyName))
                return Enumerable.Empty<string>();

            return _errors[propertyName];
        }

        private void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        private void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName))
                _errors[propertyName] = new List<string>();

            if (!_errors[propertyName].Contains(error))
            {
                _errors[propertyName].Add(error);
                OnErrorsChanged(propertyName);
            }
        }

        private void ClearErrors(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
                OnErrorsChanged(propertyName);
            }
        }

        private bool ValidateNoiseLevel(float value, [CallerMemberName] string propertyName = "")
        {
            ClearErrors(propertyName);

            if (value < 0f || value > 1f)
            {
                AddError(propertyName, "Уровень шума должен быть между 0 и 1");
                return false;
            }

            return true;
        }

        private bool ValidateTemperature(float value, [CallerMemberName] string propertyName = "")
        {
            ClearErrors(propertyName);

            if (value < -100f || value > 100f)
            {
                AddError(propertyName, "Температура должна быть между -100 и 100°C");
                return false;
            }

            return true;
        }

        public float Temperature
        {
            get => _settings.TemperatureCelsius;
            set
            {
                if (ValidateTemperature(value))
                {
                    var oldValue = _settings.TemperatureCelsius;
                    _settings.TemperatureCelsius = value;
                    ChangeLogger.LogChange(nameof(Temperature), oldValue, value);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SoundSpeed)); // Скорость звука зависит от температуры
                }
            }
        }

        public float NoiseLevel
        {
            get => _settings.NoiseLevel;
            set
            {
                if (ValidateNoiseLevel(value))
                {
                    var oldValue = _settings.NoiseLevel;
                    _settings.NoiseLevel = value;
                    ChangeLogger.LogChange(nameof(NoiseLevel), oldValue, value);
                    OnPropertyChanged();
                }
            }
        }

        public float SoundSpeed => _settings.SoundSpeed;

        public void ValidateAll()
        {
            ValidateTemperature(Temperature, nameof(Temperature));
            ValidateNoiseLevel(NoiseLevel, nameof(NoiseLevel));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}