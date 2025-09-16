using PAAOM_Common.Models.Interfaces;
using PAAOM_Common.Models;
using PAAOM_Server.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PAAOM_Server.ViewModels
{
	public class AudioSourceViewModel : INotifyPropertyChanged
	{
		private readonly IAudioSource _source;

		public event PropertyChangedEventHandler? PropertyChanged;

		public AudioSourceViewModel(IAudioSource source)
		{
			_source = source ?? throw new ArgumentNullException(nameof(source));
		}

		public float X
		{
			get => (float)_source.Position.X;
			set
			{
				var oldValue = _source.Position.X;
				_source.Position = new Point3D(value, _source.Position.Y, _source.Position.Z);
				ChangeLogger.LogChange(nameof(X), oldValue, value);
				OnPropertyChanged();
			}
		}
		public float Y
		{
			get => (float)_source.Position.Y;
			set
			{
				var oldValue = _source.Position.Y;
				_source.Position = new Point3D(_source.Position.X, value, _source.Position.Z);
				ChangeLogger.LogChange(nameof(Y), oldValue, value);
				OnPropertyChanged();
			}
		}

		public float Z
		{
			get => (float)_source.Position.Z;
			set
			{
				var oldValue = _source.Position.Z;
				_source.Position = new Point3D(_source.Position.X, _source.Position.Y, value);
				ChangeLogger.LogChange(nameof(Z), oldValue, value);
				OnPropertyChanged();
			}
		}

		public float Frequency
		{
			get => (float)_source.Frequency;
			set
			{
				var oldValue = _source.Frequency;
				_source.Frequency = value;
				ChangeLogger.LogChange(nameof(Frequency), oldValue, value);
				OnPropertyChanged();
			}
		}

		public float Amplitude
		{
			get => (float)_source.Amplitude;
			set
			{
				var oldValue = _source.Amplitude;
				_source.Amplitude = value;
				ChangeLogger.LogChange(nameof(Amplitude), oldValue, value);
				OnPropertyChanged();
			}
		}

		public float Phase
		{
			get => (float)_source.Phase;
			set
			{
				var oldValue = _source.Phase;
				_source.Phase = value;
				ChangeLogger.LogChange(nameof(Phase), oldValue, value);
				OnPropertyChanged();
			}
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
