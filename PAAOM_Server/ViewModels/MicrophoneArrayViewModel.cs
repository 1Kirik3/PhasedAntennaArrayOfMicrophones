using PAAOM_Server.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Media3D;

namespace PAAOM_Server.ViewModels
{
	public class MicrophoneArrayViewModel: INotifyPropertyChanged
	{
		private readonly MicrophoneArray _array;

		public event PropertyChangedEventHandler? PropertyChanged;

		public MicrophoneArrayViewModel(MicrophoneArray array)
		{
			_array = array ?? throw new ArgumentNullException(nameof(array));
			_array.GeometryUpdated += OnGeometryUpdated;
		}

		~MicrophoneArrayViewModel()
		{
			_array.GeometryUpdated -= OnGeometryUpdated;
		}

		public void UpdateGeometry()
		{
			_array.UpdateGeometry();
		}

		private void OnGeometryUpdated(object? sender, EventArgs e)
		{
			OnPropertyChanged(nameof(Radius));
			OnPropertyChanged(nameof(CenterX));
			OnPropertyChanged(nameof(CenterY));
			OnPropertyChanged(nameof(CenterZ));
			OnPropertyChanged(nameof(MicrophonesCount));
		}

		public float CenterX
		{
			get => (float)_array.ArrayCenter.X;
			set { _array.ArrayCenter = new Point3D(value, _array.ArrayCenter.Y, _array.ArrayCenter.Z); OnPropertyChanged(); }
		}

		public float CenterY
		{
			get => (float)_array.ArrayCenter.Y;
			set { _array.ArrayCenter = new Point3D(_array.ArrayCenter.X, value, _array.ArrayCenter.Z); OnPropertyChanged(); }
		}

		public float CenterZ
		{
			get => (float)_array.ArrayCenter.X;
			set { _array.ArrayCenter = new Point3D(_array.ArrayCenter.X, _array.ArrayCenter.Y, value); OnPropertyChanged(); }
		}

		public float Radius
		{
			get => _array.Radius;
			set { _array.Radius = value; OnPropertyChanged(); }
		}

		public int MicrophonesCount => _array.Microphones.Count;

		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}


	}
}
