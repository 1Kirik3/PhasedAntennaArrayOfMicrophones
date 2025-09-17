using System.Text.Json.Serialization;

namespace PAAOM_Common.Models
{
    public class AppSettings
    {
        public AudioSourceSettings AudioSource { get; set; } = new AudioSourceSettings();
        public EnvironmentSettings Environment { get; set; } = new EnvironmentSettings();
        public MicrophoneArraySettings MicrophoneArray { get; set; } = new MicrophoneArraySettings();
        public NetworkSettings Network { get; set; } = new NetworkSettings();
    }

    public class AudioSourceSettings
    {
        public Point3D Position { get; set; } = new Point3D(5, 5, 0);
        public double Frequency { get; set; } = 500.0; 
        public double Amplitude { get; set; } = 1.0; 
        public double Phase { get; set; } = 1.0; 
    }

    public class EnvironmentSettings
    {
        public float TemperatureCelsius { get; set; } = 20f;
        public float NoiseLevel { get; set; } = 0.05f;
    }

    public class MicrophoneArraySettings
    {
        public Point3D ArrayCenter { get; set; } = new Point3D(0, 0, 0);
        public float Radius { get; set; } = 0.5f;
        public int MicrophonesCount { get; set; } = 8;
    }

    public class NetworkSettings
    {
        public string LocalIP { get; set; } = "127.0.0.1";
        public string LocalPort { get; set; } = "5000";
        public string RemoteIP { get; set; } = "127.0.0.1";
        public string RemotePort { get; set; } = "5001";
    }
}