using PAAOM_Common.Models.Interfaces;
using System;

namespace PAAOM_Common.Models
{
	public class Environment : IEnvironment
	{
		private float _temperatureCelsius = 20f;
		private float _noiseLevel = 0.05f;

		public event Action<float> TemperatureChanged;
		public event Action<float> NoiseLevelChanged;

		public Environment()
		{
		}

		public Environment(float temperatureCelsius, float noiseLevel)
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
					TemperatureChanged?.Invoke(value);
				}
			}
		}

		public float SoundSpeed => 331f + 0.6f * TemperatureCelsius;

		public float NoiseLevel
		{
			get => _noiseLevel;
			set
			{
				if (value < 0f || value > 1f)
				{
					throw new ArgumentOutOfRangeException(nameof(value), "Noise level must be between 0 and 1.");
				}

				if (_noiseLevel != value)
				{
					_noiseLevel = value;
					NoiseLevelChanged?.Invoke(value);
				}
			}
		}
	}
}