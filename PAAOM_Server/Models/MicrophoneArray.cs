using System.ComponentModel;
using System.Windows.Media.Media3D;

namespace PAAOM_Server.Models
{
	public class MicrophoneArray : INotifyPropertyChanged
	{
		private int _microphonesCount = 8; public float Radius { get; set; } = 0.5f;
		public Point3D ArrayCenter { get; set; } = new Point3D(0, 0, 0);
		public List<Microphone> Microphones { get; private set; } = new List<Microphone>();

		public event EventHandler? GeometryUpdated;
		public event PropertyChangedEventHandler? PropertyChanged;

		private EnvironmentSettings _environment;
		private AudioSource _source;


		public MicrophoneArray(EnvironmentSettings environmentSettings, AudioSource audioSource)
		{
			_environment = environmentSettings;
			_source = audioSource;
			InitilizeMicrophones();
		}

		public void UpdateGeometry()
		{
			foreach (var mic in Microphones)
			{
				mic.UpdateSourceParameters(_source.Position, _environment.SoundSpeed);
			}

			GeometryUpdated?.Invoke(this, EventArgs.Empty);
		}

		public int MicrophonesCount
		{
			get => _microphonesCount;
			set
			{
				if (_microphonesCount != value)
				{
					_microphonesCount = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MicrophonesCount)));
					InitilizeMicrophones();
				}
			}
		}

		public void InitilizeMicrophones()
		{
			Microphones.Clear();

			for (int i = 0; i < MicrophonesCount; i++)
			{
				double angle = 2 * Math.PI * i / MicrophonesCount;
				double x = ArrayCenter.X + Radius * Math.Cos(angle);
				double y = ArrayCenter.Y + Radius * Math.Sin(angle);

				var mic = new Microphone(new Point3D(x, y, ArrayCenter.Z));
				mic.UpdateSourceParameters(_source.Position, _environment.SoundSpeed);
				Microphones.Add(mic);
			}

			UpdateGeometry();
		}

	}
}
