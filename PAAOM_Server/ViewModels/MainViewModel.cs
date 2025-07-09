using PAAOM_Server.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace PAAOM_Server.ViewModels
{
	public class MainViewModel: INotifyPropertyChanged
	{
		private readonly EnvironmentSettings _environmentSettings;
		private readonly AudioSource _audioSource;
		private readonly MicrophoneArray _microphoneArray;

		public EnvironmentSettingsViewModel EnvironmentSettingsVM { get; }
		public AudioSourceViewModel AudioSourceVM { get; }
		public MicrophoneArrayViewModel MicrophoneArrayVM { get; }

		public event PropertyChangedEventHandler? PropertyChanged;

		public ICommand ApplySettingsCommand { get; }
		public ICommand StartSimulationCommand { get; }
		public ICommand StopSimulationCommand { get; }

		//private bool _isSimulationRunning;
		//public bool IsSimulationRunning
		//{
		//	get => _isSimulationRunning;
		//	set
		//	{
		//		if (_isSimulationRunning != value)
		//		{
		//			_isSimulationRunning = value;
		//			OnPropertyChanged();
		//			CommandManager.InvalidateRequerySuggested();
		//		}
		//	}
		//}

		public MainViewModel(
			EnvironmentSettings envSettings,
			AudioSource audioSource,
			MicrophoneArray microphoneArray)
		{
			// Проверка входных параметров
			if (envSettings == null) throw new ArgumentNullException(nameof(envSettings));
			if (audioSource == null) throw new ArgumentNullException(nameof(audioSource));
			if (microphoneArray == null) throw new ArgumentNullException(nameof(microphoneArray));

			// Инициализация ViewModels
			EnvironmentSettingsVM = new EnvironmentSettingsViewModel(envSettings);
			AudioSourceVM = new AudioSourceViewModel(audioSource);
			MicrophoneArrayVM = new MicrophoneArrayViewModel(microphoneArray);
		}

		private void ApplySettings()
		{
			// Обновляем параметры среды
			_environmentSettings.TemperatureCelsius = EnvironmentSettingsVM.Temperature;
			_environmentSettings.NoiseLevel = EnvironmentSettingsVM.NoiseLevel;

			// Обновляем параметры источника
			_audioSource.Position = new Point3D(
				AudioSourceVM.X,
				AudioSourceVM.Y,
				AudioSourceVM.Z);
			_audioSource.Frequency = AudioSourceVM.Frequency;
			_audioSource.Amplitude = AudioSourceVM.Amplitude;
			_audioSource.Phase = AudioSourceVM.Phase;

			// Обновляем геометрию массива
			_microphoneArray.ArrayCenter = new Point3D(
				MicrophoneArrayVM.CenterX,
				MicrophoneArrayVM.CenterY,
				MicrophoneArrayVM.CenterZ);
			_microphoneArray.Radius = MicrophoneArrayVM.Radius;

			_microphoneArray.UpdateGeometry();
		}

		//private void StartSimulation()
		//{
		//	IsSimulationRunning = true;
		//	// Здесь будет логика запуска симуляции
		//}

		//private void StopSimulation()
		//{
		//	IsSimulationRunning = false;
		//	// Здесь будет логика остановки симуляции
		//}

		//private bool CanExecuteCommands()
		//{
		//	return !IsSimulationRunning;
		//}

		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
