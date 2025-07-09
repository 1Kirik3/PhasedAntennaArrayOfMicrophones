using PAAOM_Server.Models;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Media3D;

namespace PAAOM_Server.ViewModels
{
	public class AudioSourceViewModel : INotifyPropertyChanged
	{
		private readonly AudioSource _source;

		public event PropertyChangedEventHandler? PropertyChanged;

		public AudioSourceViewModel(AudioSource source)
		{
			_source = source ?? throw new ArgumentNullException(nameof(source));
		}

		public float X
		{
			get => (float)_source.Position.X;
			set { _source.Position = new Point3D(value, _source.Position.Y, _source.Position.Z); OnPropertyChanged(); }
		}
		public float Y
		{
			get => (float)_source.Position.Y;
			set { _source.Position = new Point3D(_source.Position.X, value, _source.Position.Z); OnPropertyChanged(); }
		}

		public float Z
		{
			get => (float)_source.Position.Z;
			set { _source.Position = new Point3D(_source.Position.X, _source.Position.Y, value); OnPropertyChanged(); }
		}

		public float Frequency
		{
			get => (float)_source.Frequency;
			set { _source.Frequency = value; OnPropertyChanged(); }
		}

		public float Amplitude
		{
			get => (float)_source.Amplitude;
			set { _source.Amplitude = value; OnPropertyChanged(); }
		}

		public float Phase
		{
			get => (float)_source.Phase;
			set { _source.Phase = value; OnPropertyChanged(); }
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
