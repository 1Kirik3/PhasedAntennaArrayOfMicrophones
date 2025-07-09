using System.Windows.Media.Media3D;

namespace PAAOM_Server.Models
{
	public class Microphone
	{
		public Point3D Position { get; set; }
		public float DistanceToSource { get; set; }
		public float DelayToSource { get; set; }

		public Microphone(Point3D point3D) 
		{
			Position = point3D;
			//DistanceToSource = delayToSource;
			//DelayToSource = delayToSource;
		}

		public void UpdateSourceParameters(Point3D sourcePos, float soundSpeed)
		{
			DistanceToSource = (float)Math.Sqrt(
				Math.Pow(sourcePos.X - Position.X, 2) +
				Math.Pow(sourcePos.Y - Position.Y, 2) +
				Math.Pow(sourcePos.Z - Position.Z, 2));

			DelayToSource = DistanceToSource / soundSpeed;
		}
	}
}
