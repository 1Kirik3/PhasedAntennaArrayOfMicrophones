using PAAOM_Common.Models.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PAAOM_Server.ViewModels
{
	public class SettingsViewModel : INotifyPropertyChanged
	{
		public EnvironmentSettingsViewModel EnvironmentSettings { get; }
		public AudioSourceViewModel AudioSource { get; }
		public MicrophoneArrayViewModel MicrophoneArray { get; }

		public ICommand ApplySettingsCommand { get; }

		public SettingsViewModel(
			IEnvironmentSettings envSettings,
			IAudioSource audioSource,
			IMicrophoneArray microphoneArray)
		{
			EnvironmentSettings = new EnvironmentSettingsViewModel(envSettings);
			AudioSource = new AudioSourceViewModel(audioSource);
			MicrophoneArray = new MicrophoneArrayViewModel(microphoneArray);

			ApplySettingsCommand = new RelayCommand(ApplySettings);
		}

		private void ApplySettings()
		{
			MicrophoneArray.UpdateGeometry();
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}