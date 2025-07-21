using PAAOM_Server.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace PAAOM_Server.ViewModels
{
	public class MainViewModel: INotifyPropertyChanged
	{
		public EnvironmentSettingsViewModel EnvironmentSettingsVM { get; }
		public AudioSourceViewModel AudioSourceVM { get; }
		public MicrophoneArrayViewModel MicrophoneArrayVM { get; }

		private readonly SignalGenerator _signalGenerator;
		private readonly MicrophoneArray _array;
		private readonly AudioSource _source;
		private readonly EnvironmentSettings _environment;


		public event PropertyChangedEventHandler? PropertyChanged;

		public ICommand ApplySettingsCommand { get; }
		public ICommand StartSimulationCommand { get; }
		public ICommand StopSimulationCommand { get; }
		public ICommand UpdateSignalsCommand { get; }

		private ObservableCollection<MicrophoneSignalViewModel> _microphoneSignals { get; }


		public MainViewModel(
			EnvironmentSettings envSettings,
			AudioSource audioSource,
			MicrophoneArray microphoneArray)
		{
			if (envSettings == null) throw new ArgumentNullException(nameof(envSettings));
			if (audioSource == null) throw new ArgumentNullException(nameof(audioSource));
			if (microphoneArray == null) throw new ArgumentNullException(nameof(microphoneArray));

			_environment = envSettings;
			_source = audioSource;
			_array = microphoneArray;
			_signalGenerator = new SignalGenerator();

			EnvironmentSettingsVM = new EnvironmentSettingsViewModel(envSettings);
			AudioSourceVM = new AudioSourceViewModel(audioSource);
			MicrophoneArrayVM = new MicrophoneArrayViewModel(microphoneArray);

			_microphoneSignals = new ObservableCollection<MicrophoneSignalViewModel>();

			UpdateSignalsCommand = new RelayCommand(UpdateSignals);
		}

		public void UpdateSignals()
		{
			try
			{
				var signals = _signalGenerator.GenerateSignals(_array, _source, _environment);

				_microphoneSignals.Clear();

				for (int i = 0; i < signals.Count; i++)
				{
					var signalVm = new MicrophoneSignalViewModel(i, signals[i]);
					_microphoneSignals.Add(signalVm);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Ошибка обновления: {ex.Message}");
				MessageBox.Show($"UpdateSignals error: {ex.Message}", "Error",
							  MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
