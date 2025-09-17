using PAAOM_Common.Models.Interfaces;
using System.Windows.Media.Media3D;

namespace PAAOM_Common.Models
{
    public class AudioSource : IAudioSource
    {
        public Point3D Position { get; set; } = new Point3D(5, 5, 0);
        public double Frequency { get; set; } = 500.0;
        public double Amplitude { get; set; } = 1.0;
        public double Phase { get; set; } = 1.0;

        public AudioSource()
        {
        }

        public AudioSource(Point3D position, double frequency, double amplitude, double phase)
        {
            Position = position;
            Frequency = frequency;
            Amplitude = amplitude;
            Phase = phase;
        }
    }
}