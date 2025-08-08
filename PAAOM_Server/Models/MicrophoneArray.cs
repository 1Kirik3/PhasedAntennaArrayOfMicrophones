using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace PAAOM_Server.Models
{
	public class MicrophoneArray
	{
		private int _microphonesCount = 8;
		private float _radius = 0.5f;
		private Point3D _arrayCenter = new Point3D(0, 0, 0);

		public event Action<int>? MicrophonesCountChanged;
		public event Action<float>? RadiusChanged;
		public event Action<Point3D>? ArrayCenterChanged;
		public event EventHandler? GeometryUpdated;

		public List<Microphone> Microphones { get; } = new List<Microphone>();
		public float Radius
		{
			get => _radius;
			set
			{
				if (_radius != value)
				{
					_radius = value;
					RadiusChanged?.Invoke(value);
					InitilizeMicrophones();
				}
			}
		}

		public Point3D ArrayCenter
		{
			get => _arrayCenter;
			set
			{
				if (_arrayCenter != value)
				{
					_arrayCenter = value;
					ArrayCenterChanged?.Invoke(value);
					InitilizeMicrophones();
				}
			}
		}

		private readonly EnvironmentSettings _environment;
		private readonly AudioSource _source;

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
					MicrophonesCountChanged?.Invoke(value);
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