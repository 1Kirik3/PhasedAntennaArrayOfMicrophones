
namespace PAAOM_Common.Models
{
	public class Microphone
	{
		public Point3D Position { get; set; }

		public Microphone(Point3D position)
		{
			Position = position;
		}
	}
}