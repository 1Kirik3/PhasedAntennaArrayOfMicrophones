using PAAOM_Server.Models;
using PAAOM_Server.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Media3D;

namespace PAAOM_Server.ViewModels
{
	public class MicrophoneArrayViewModel: INotifyPropertyChanged
	{
		private readonly MicrophoneArray _array;

		public event PropertyChangedEventHandler? PropertyChanged;
		public int MicrophonesCount => _array.Microphones.Count;

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
			set
			{
				var oldValue = _array.ArrayCenter.X;
				_array.ArrayCenter = new Point3D(value, _array.ArrayCenter.Y, _array.ArrayCenter.Z);
				ChangeLogger.LogChange(nameof(CenterX), oldValue, value);
				OnPropertyChanged();
			}
		}

		public float CenterY
		{
			get => (float)_array.ArrayCenter.Y;
			set
			{
				var oldValue = _array.ArrayCenter.Y;
				_array.ArrayCenter = new Point3D(_array.ArrayCenter.X, value, _array.ArrayCenter.Z);
				ChangeLogger.LogChange(nameof(CenterY), oldValue, value);
				OnPropertyChanged();
			}
		}

		public float CenterZ
		{
			get => (float)_array.ArrayCenter.Z;
			set
			{
				var oldValue = _array.ArrayCenter.Z;
				_array.ArrayCenter = new Point3D(_array.ArrayCenter.X, _array.ArrayCenter.Y, value);
				ChangeLogger.LogChange(nameof(CenterZ), oldValue, value);
				OnPropertyChanged();
			}
		}

		public float Radius
		{
			get => _array.Radius;
			set
			{
				var oldValue = _array.Radius;
				_array.Radius = value;
				ChangeLogger.LogChange(nameof(Radius), oldValue, value);
				OnPropertyChanged();
			}
		}

		public int MicrophoneCount
		{
			get => _array.MicrophonesCount;
			set
			{
				if (_array.MicrophonesCount != value)
				{
					_array.MicrophonesCount = value;
					_array.InitilizeMicrophones();
					OnPropertyChanged();
				}
			}
		}


		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}


	}
}
