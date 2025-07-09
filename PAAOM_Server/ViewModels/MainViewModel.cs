using PAAOM_Server.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PAAOM_Server.ViewModels
{
	public class MainViewModel: INotifyPropertyChanged
	{

		public EnvironmentSettingsViewModel EnvironmentSettingsVM { get; }
		public AudioSourceViewModel AudioSourceVM { get; }
		public MicrophoneArrayViewModel MicrophoneArrayVM { get; }

		public event PropertyChangedEventHandler? PropertyChanged;

		public ICommand ApplySettingsCommand { get; }
		public ICommand StartSimulationCommand { get; }
		public ICommand StopSimulationCommand { get; }


		public MainViewModel(
			EnvironmentSettings envSettings,
			AudioSource audioSource,
			MicrophoneArray microphoneArray)
		{
			if (envSettings == null) throw new ArgumentNullException(nameof(envSettings));
			if (audioSource == null) throw new ArgumentNullException(nameof(audioSource));
			if (microphoneArray == null) throw new ArgumentNullException(nameof(microphoneArray));

			EnvironmentSettingsVM = new EnvironmentSettingsViewModel(envSettings);
			AudioSourceVM = new AudioSourceViewModel(audioSource);
			MicrophoneArrayVM = new MicrophoneArrayViewModel(microphoneArray);
		}


		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
