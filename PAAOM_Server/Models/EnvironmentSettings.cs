
namespace PAAOM_Server.Models
{
	public class EnvironmentSettings
	{
		public float TemperatureCelsius { get; set; } = 20f;
		public float SoundSpeed => 331f + 0.6f * TemperatureCelsius;
		public float NoiseLevel { get; set; } = 0.05f;

		public EnvironmentSettings()
		{
		}

		public EnvironmentSettings(float temperatureCelsius, float noiseLevel)
		{
			TemperatureCelsius = temperatureCelsius;
			NoiseLevel = noiseLevel;
		}
	}
}
