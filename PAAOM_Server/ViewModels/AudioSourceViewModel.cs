using PAAOM_Common.Models;
using PAAOM_Common.Models.Interfaces;
using PAAOM_Common.ValidationRules;
using PAAOM_Server.Services;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PAAOM_Server.ViewModels
{
    public class AudioSourceViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly IAudioSource _source;
        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public AudioSourceViewModel(IAudioSource source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public void RefreshAllProperties()
        {
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(Z));
            OnPropertyChanged(nameof(Frequency));
            OnPropertyChanged(nameof(Amplitude));
            OnPropertyChanged(nameof(Phase));
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

        private bool ValidateCoordinate(float value, [CallerMemberName] string propertyName = "")
        {
            ClearErrors(propertyName);

            if (value < (float)ValidationConstants.MinCoordinate || value > (float)ValidationConstants.MaxCoordinate)
            {
                AddError(propertyName, $"Координата должна быть в пределах ±1000 м");
                return false;
            }

            return true;
        }

        private bool ValidateFrequency(float value, [CallerMemberName] string propertyName = "")
        {
            ClearErrors(propertyName);

            if (value < 0f || value > (float)ValidationConstants.MaxFrequency)
            {
                AddError(propertyName, $"Частота не должна превышать {ValidationConstants.MaxFrequency} Гц (по теореме Найквиста)");
                return false;
            }

            return true;
        }

        private bool ValidateAmplitude(float value, [CallerMemberName] string propertyName = "")
        {
            ClearErrors(propertyName);

            if (value < (float)ValidationConstants.MinAmplitude || value > (float)ValidationConstants.MaxAmplitude)
            {
                AddError(propertyName, $"Амплитуда должна быть между {ValidationConstants.MinAmplitude} и {ValidationConstants.MaxAmplitude}");
                return false;
            }

            return true;
        }

        private bool ValidatePhase(float value, [CallerMemberName] string propertyName = "")
        {
            ClearErrors(propertyName);

            if (value < (float)ValidationConstants.MinPhase || value > (float)ValidationConstants.MaxPhase)
            {
                AddError(propertyName, $"Фаза должна быть между -π и π радиан");
                return false;
            }

            return true;
        }

        public float X
        {
            get => (float)_source.Position.X;
            set
            {
                if (ValidateCoordinate(value))
                {
                    var oldValue = _source.Position.X;
                    _source.Position = new Point3D(value, _source.Position.Y, _source.Position.Z);
                    ChangeLogger.LogChange(nameof(X), oldValue, value);
                    OnPropertyChanged();
                }
            }
        }

        public float Y
        {
            get => (float)_source.Position.Y;
            set
            {
                if (ValidateCoordinate(value))
                {
                    var oldValue = _source.Position.Y;
                    _source.Position = new Point3D(_source.Position.X, value, _source.Position.Z);
                    ChangeLogger.LogChange(nameof(Y), oldValue, value);
                    OnPropertyChanged();
                }
            }
        }

        public float Z
        {
            get => (float)_source.Position.Z;
            set
            {
                if (ValidateCoordinate(value))
                {
                    var oldValue = _source.Position.Z;
                    _source.Position = new Point3D(_source.Position.X, _source.Position.Y, value);
                    ChangeLogger.LogChange(nameof(Z), oldValue, value);
                    OnPropertyChanged();
                }
            }
        }

        public float Frequency
        {
            get => (float)_source.Frequency;
            set
            {
                if (ValidateFrequency(value))
                {
                    var oldValue = _source.Frequency;
                    _source.Frequency = value;
                    ChangeLogger.LogChange(nameof(Frequency), oldValue, value);
                    OnPropertyChanged();
                }
            }
        }

        public float Amplitude
        {
            get => (float)_source.Amplitude;
            set
            {
                if (ValidateAmplitude(value))
                {
                    var oldValue = _source.Amplitude;
                    _source.Amplitude = value;
                    ChangeLogger.LogChange(nameof(Amplitude), oldValue, value);
                    OnPropertyChanged();
                }
            }
        }

        public float Phase
        {
            get => (float)_source.Phase;
            set
            {
                if (ValidatePhase(value))
                {
                    var oldValue = _source.Phase;
                    _source.Phase = value;
                    ChangeLogger.LogChange(nameof(Phase), oldValue, value);
                    OnPropertyChanged();
                }
            }
        }

        public void ValidateAll()
        {
            ValidateCoordinate(X, nameof(X));
            ValidateCoordinate(Y, nameof(Y));
            ValidateCoordinate(Z, nameof(Z));
            ValidateFrequency(Frequency, nameof(Frequency));
            ValidateAmplitude(Amplitude, nameof(Amplitude));
            ValidatePhase(Phase, nameof(Phase));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}