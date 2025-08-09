using PAAOM_Server.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace PAAOM_Server.Models
{
	public class MicrophoneArray : IMicrophoneArray
	{
		private int _microphonesCount = 8;
		private float _radius = 0.5f;
		private Point3D _arrayCenter = new Point3D(0, 0, 0);
		private readonly List<Microphone> _microphones = new List<Microphone>();

		public event Action<int>? MicrophonesCountChanged;
		public event Action<float>? RadiusChanged;
		public event Action<Point3D>? ArrayCenterChanged;
		public event EventHandler? GeometryUpdated;

		private readonly IEnvironmentSettings _environment;
		private readonly IAudioSource _source;

		IReadOnlyList<Microphone> IMicrophoneArray.Microphones => _microphones.AsReadOnly();

		public float Radius
		{
			get => _radius;
			set
			{
				if (value <= 0)
					throw new ArgumentOutOfRangeException(nameof(value), "Radius must be positive");

				if (Math.Abs(_radius - value) > float.Epsilon)
				{
					_radius = value;
					RadiusChanged?.Invoke(value);
					InitializeMicrophones();
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
					InitializeMicrophones();
				}
			}
		}

		public int MicrophonesCount
		{
			get => _microphonesCount;
			set
			{
				if (value <= 0)
					throw new ArgumentOutOfRangeException(nameof(value), "Microphones count must be positive");

				if (_microphonesCount != value)
				{
					_microphonesCount = value;
					MicrophonesCountChanged?.Invoke(value);
					InitializeMicrophones();
				}
			}
		}

		public MicrophoneArray(IEnvironmentSettings environmentSettings, IAudioSource audioSource)
		{
			_environment = environmentSettings ?? throw new ArgumentNullException(nameof(environmentSettings));
			_source = audioSource ?? throw new ArgumentNullException(nameof(audioSource));
			InitializeMicrophones();
		}

		public void UpdateGeometry()
		{
			foreach (var mic in _microphones)
			{
				mic.UpdateSourceParameters(_source.Position, _environment.SoundSpeed);
			}
			GeometryUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void InitializeMicrophones()
		{
			_microphones.Clear();

			for (int i = 0; i < _microphonesCount; i++)
			{
				double angle = 2 * Math.PI * i / _microphonesCount;
				double x = _arrayCenter.X + _radius * Math.Cos(angle);
				double y = _arrayCenter.Y + _radius * Math.Sin(angle);

				var mic = new Microphone(new Point3D(x, y, _arrayCenter.Z));
				mic.UpdateSourceParameters(_source.Position, _environment.SoundSpeed);
				_microphones.Add(mic);
			}

			UpdateGeometry();
		}
	}
}