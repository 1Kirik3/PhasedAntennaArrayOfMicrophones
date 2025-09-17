using PAAOM_Common.Models.Interfaces;
using System;
using System.Collections.Generic;

namespace PAAOM_Common.Models
{
	public class MicrophoneArray : IMicrophoneArray
	{
		private int _microphonesCount = 8;
		private float _radius = 0.5f;
		private Point3D _arrayCenter = new Point3D(0, 0, 0);
		private readonly List<Microphone> _microphones = new List<Microphone>();

		public event Action<int> MicrophonesCountChanged;
		public event Action<float> RadiusChanged;
		public event Action<Point3D> ArrayCenterChanged;
		public event EventHandler GeometryUpdated;

		private readonly IEnvironment _environment;
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

		public MicrophoneArray(IEnvironment environmentSettings, IAudioSource audioSource)
		{
			_environment = environmentSettings ?? throw new ArgumentNullException(nameof(environmentSettings));
			_source = audioSource ?? throw new ArgumentNullException(nameof(audioSource));
			InitializeMicrophones();
		}

		public void UpdateGeometry()
		{
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

				_microphones.Add(new Microphone(new Point3D(x, y, _arrayCenter.Z)));
			}

			UpdateGeometry();
		}

		public float GetDistanceToSource(Microphone microphone)
		{
			return (float)Math.Sqrt(
				Math.Pow(_source.Position.X - microphone.Position.X, 2) +
				Math.Pow(_source.Position.Y - microphone.Position.Y, 2) +
				Math.Pow(_source.Position.Z - microphone.Position.Z, 2));
		}

		public float GetDelayToSource(Microphone microphone)
		{
			return GetDistanceToSource(microphone) / _environment.SoundSpeed;
		}
	}
}