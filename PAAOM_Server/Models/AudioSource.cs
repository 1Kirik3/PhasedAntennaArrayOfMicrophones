using System.Windows.Media.Media3D;

namespace PAAOM_Server.Models
{
	public class AudioSource
	{
		public Point3D Position { get; set; } = new Point3D(5, 5, 0);
		public float Frequency { get; set; } = 500f;
		public float Amplitude { get; set; } = 1.0f;
		public float Phase { get; set; } = 1.0f;

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
