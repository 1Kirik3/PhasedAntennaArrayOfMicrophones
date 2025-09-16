using PAAOM_Common.Models.Interfaces;

namespace PAAOM_Common.Models
{
	public class AudioSource : IAudioSource
	{
		public Point3D Position { get; set; } = new Point3D(5, 5, 0);
		public float Frequency { get; set; } = 500f;
		public float Amplitude { get; set; } = 1.0f;
		public float Phase { get; set; } = 1.0f;

		double IAudioSource.Frequency { get => Frequency; set => Frequency = (float)value; }
		double IAudioSource.Amplitude { get => Amplitude; set => Amplitude = (float)value; }
		double IAudioSource.Phase { get => Phase; set => Phase = (float)value; }

		public AudioSource()
		{
		}

		public AudioSource(Point3D position, float frequency, float amplitude, float phase)
		{
			Position = position;
			Frequency = frequency;
			Amplitude = amplitude;
			Phase = phase;
		}

	}
}
