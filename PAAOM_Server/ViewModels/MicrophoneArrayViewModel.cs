using PAAOM_Common.Models;
using PAAOM_Common.Models.Interfaces;
using PAAOM_Common.ValidationRules;
using PAAOM_Server.Services;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PAAOM_Server.ViewModels
{
    public class MicrophoneArrayViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly IMicrophoneArray _array;
        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public MicrophoneArrayViewModel(IMicrophoneArray array)
        {
            _array = array ?? throw new ArgumentNullException(nameof(array));

            _array.MicrophonesCountChanged += OnMicrophonesCountChanged;
            _array.RadiusChanged += OnRadiusChanged;
            _array.ArrayCenterChanged += OnArrayCenterChanged;
            _array.GeometryUpdated += OnGeometryUpdated;
        }

        ~MicrophoneArrayViewModel()
        {
            _array.MicrophonesCountChanged -= OnMicrophonesCountChanged;
            _array.RadiusChanged -= OnRadiusChanged;
            _array.ArrayCenterChanged -= OnArrayCenterChanged;
            _array.GeometryUpdated -= OnGeometryUpdated;
        }

        private void OnMicrophonesCountChanged(int count)
        {
            OnPropertyChanged(nameof(MicrophoneCount));
            OnPropertyChanged(nameof(MicrophonesCount));
            ClearErrors(nameof(MicrophoneCount));
        }

        private void OnRadiusChanged(float radius)
        {
            OnPropertyChanged(nameof(Radius));
            ClearErrors(nameof(Radius));
        }

        private void OnArrayCenterChanged(Point3D center)
        {
            OnPropertyChanged(nameof(CenterX));
            OnPropertyChanged(nameof(CenterY));
            OnPropertyChanged(nameof(CenterZ));
            ClearErrors(nameof(CenterX));
            ClearErrors(nameof(CenterY));
            ClearErrors(nameof(CenterZ));
        }

        private void OnGeometryUpdated(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(Radius));
            OnPropertyChanged(nameof(CenterX));
            OnPropertyChanged(nameof(CenterY));
            OnPropertyChanged(nameof(CenterZ));
            OnPropertyChanged(nameof(MicrophonesCount));
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

        private bool ValidateRadius(float value, [CallerMemberName] string propertyName = "")
        {
            ClearErrors(propertyName);

            if (value < (float)ValidationConstants.MinRadius || value > (float)ValidationConstants.MaxRadius)
            {
                AddError(propertyName, $"Радиус массива должен быть между {ValidationConstants.MinRadius} и {ValidationConstants.MaxRadius} м");
                return false;
            }

            return true;
        }

        private bool ValidateMicrophoneCount(int value, [CallerMemberName] string propertyName = "")
        {
            ClearErrors(propertyName);

            if (value != 4 && value != 8 && value != 16 && value != 32)
            {
                AddError(propertyName, "Количество микрофонов должно быть 4, 8, 16 или 32");
                return false;
            }

            return true;
        }

        public float CenterX
        {
            get => (float)_array.ArrayCenter.X;
            set
            {
                if (ValidateCoordinate(value))
                {
                    var oldValue = _array.ArrayCenter.X;
                    _array.ArrayCenter = new Point3D(value, _array.ArrayCenter.Y, _array.ArrayCenter.Z);
                    ChangeLogger.LogChange(nameof(CenterX), oldValue, value);
                    OnPropertyChanged();
                }
            }
        }

        public float CenterY
        {
            get => (float)_array.ArrayCenter.Y;
            set
            {
                if (ValidateCoordinate(value))
                {
                    var oldValue = _array.ArrayCenter.Y;
                    _array.ArrayCenter = new Point3D(_array.ArrayCenter.X, value, _array.ArrayCenter.Z);
                    ChangeLogger.LogChange(nameof(CenterY), oldValue, value);
                    OnPropertyChanged();
                }
            }
        }

        public float CenterZ
        {
            get => (float)_array.ArrayCenter.Z;
            set
            {
                if (ValidateCoordinate(value))
                {
                    var oldValue = _array.ArrayCenter.Z;
                    _array.ArrayCenter = new Point3D(_array.ArrayCenter.X, _array.ArrayCenter.Y, value);
                    ChangeLogger.LogChange(nameof(CenterZ), oldValue, value);
                    OnPropertyChanged();
                }
            }
        }

        public float Radius
        {
            get => _array.Radius;
            set
            {
                if (ValidateRadius(value))
                {
                    var oldValue = _array.Radius;
                    _array.Radius = value;
                    ChangeLogger.LogChange(nameof(Radius), oldValue, value);
                    OnPropertyChanged();
                }
            }
        }

        public int MicrophoneCount
        {
            get => _array.MicrophonesCount;
            set
            {
                if (ValidateMicrophoneCount(value))
                {
                    if (_array.MicrophonesCount != value)
                    {
                        _array.MicrophonesCount = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public int MicrophonesCount => _array.Microphones.Count;

        public void UpdateGeometry()
        {
            _array.UpdateGeometry();
        }

        public void ValidateAll()
        {
            ValidateCoordinate(CenterX, nameof(CenterX));
            ValidateCoordinate(CenterY, nameof(CenterY));
            ValidateCoordinate(CenterZ, nameof(CenterZ));
            ValidateRadius(Radius, nameof(Radius));
            ValidateMicrophoneCount(MicrophoneCount, nameof(MicrophoneCount));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}