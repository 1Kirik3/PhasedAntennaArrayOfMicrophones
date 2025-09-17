using System;

namespace PAAOM_Common.Models.Interfaces
{
	public interface IEnvironment
	{
		float TemperatureCelsius { get; set; }
		float NoiseLevel { get; set; }
		float SoundSpeed { get; }
		event Action<float> TemperatureChanged;
		event Action<float> NoiseLevelChanged;
	}
}
