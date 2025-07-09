using PAAOM_Server.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace PAAOM_Server.ViewModels
{
	public class EnvironmentSettingsViewModel : INotifyPropertyChanged
	{
		private readonly EnvironmentSettings _settings;

		public event PropertyChangedEventHandler? PropertyChanged;

		public EnvironmentSettingsViewModel(EnvironmentSettings settings)
		{
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
		}

		public float Temperature
		{
			get => _settings.TemperatureCelsius;
			set { _settings.TemperatureCelsius = value; OnPropertyChanged(); }
		}

		public float NoiseLevel
		{
			get => _settings.NoiseLevel;
			set { _settings.NoiseLevel = value; OnPropertyChanged(); }
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
