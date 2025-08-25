using System.Windows.Media.Media3D;

namespace PAAOM_Common.Models.Interfaces
{
	public interface IAudioSource
	{
		Point3D Position { get; set; }
		double Frequency { get; set; }
		double Amplitude { get; set; }
		double Phase { get; set; }
	}
}
