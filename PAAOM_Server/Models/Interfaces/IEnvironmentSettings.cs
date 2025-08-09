using System.Windows.Media.Media3D;

namespace PAAOM_Server.Models.Interfaces
{
	public interface IEnvironmentSettings
	{
		float TemperatureCelsius { get; set; }
		float NoiseLevel { get; set; }
		float SoundSpeed { get; }
		event Action<float> TemperatureChanged;
		event Action<float> NoiseLevelChanged;
	}
}
