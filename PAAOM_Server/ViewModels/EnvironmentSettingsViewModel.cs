using PAAOM_Server.Models;
using PAAOM_Server.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace PAAOM_Server.ViewModels
{
	public class EnvironmentSettingsViewModel : INotifyPropertyChanged
	{
		private readonly EnvironmentSettings _settings;

		public event PropertyChangedEventHandler? PropertyChanged;

		public EnvironmentSettingsViewModel(EnvironmentSettings settings)
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
		}

		private void OnNoiseLevelChanged(float newValue)
		{
			OnPropertyChanged(nameof(NoiseLevel));
		}

		public float Temperature
		{
			get => _settings.TemperatureCelsius;
			set
			{
				var oldValue = _settings.TemperatureCelsius;
				_settings.TemperatureCelsius = value;
				ChangeLogger.LogChange(nameof(Temperature), oldValue, value);
			}
		}

		public float NoiseLevel
		{
			get => _settings.NoiseLevel;
			set
			{
				if (value < 0f || value > 1f)
				{
					throw new ArgumentOutOfRangeException(nameof(value), "Noise level must be between 0 and 1.");
				}

				var oldValue = _settings.NoiseLevel;
				_settings.NoiseLevel = value;
				ChangeLogger.LogChange(nameof(NoiseLevel), oldValue, value);
			}
		}

		public float SoundSpeed => _settings.SoundSpeed;

		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
