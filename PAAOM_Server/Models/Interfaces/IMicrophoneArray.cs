using System.Windows.Media.Media3D;

namespace PAAOM_Server.Models.Interfaces
{
	public interface IMicrophoneArray
	{
		Point3D ArrayCenter { get; set; }
		float Radius { get; set; }
		int MicrophonesCount { get; set; }
		System.Collections.Generic.IReadOnlyList<Microphone> Microphones { get; }
		event Action<int> MicrophonesCountChanged;
		event Action<float> RadiusChanged;
		event Action<Point3D> ArrayCenterChanged;
		event EventHandler GeometryUpdated;
		void UpdateGeometry();
	}
}
