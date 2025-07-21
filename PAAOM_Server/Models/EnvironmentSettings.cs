
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PAAOM_Server.Models
{
	public class EnvironmentSettings : INotifyPropertyChanged
	{
		private float _temperatureCelsius = 20f;
		private float _noiseLevel = 0.05f;

		public event PropertyChangedEventHandler? PropertyChanged;

		public EnvironmentSettings()
		{
		}

		public EnvironmentSettings(float temperatureCelsius, float noiseLevel)
		{
			TemperatureCelsius = temperatureCelsius;
			NoiseLevel = noiseLevel;
		}

		public float TemperatureCelsius
		{
			get => _temperatureCelsius;
			set
			{
				if (_temperatureCelsius != value)
				{
					_temperatureCelsius = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(SoundSpeed));
				}
			}
		}

		public float SoundSpeed => 331f + 0.6f * TemperatureCelsius;

		public float NoiseLevel
		{
			get => _noiseLevel;
			set
			{
				if (_noiseLevel != value)
				{
					_noiseLevel = value;
					OnPropertyChanged();
				}
			}
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}


	}
}
