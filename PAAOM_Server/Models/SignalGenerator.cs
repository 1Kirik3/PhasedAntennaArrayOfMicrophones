using PAAOM_Server.Models.Interfaces;

	
namespace PAAOM_Server.Models
{
	public class SignalGenerator
	{
		private const float _righSampleRate = 10000f;
		private const int _decimationFactor = 8;
		private const float _processingIntervalMs = 100f;

		public const double OutputSampleRate = _righSampleRate / _decimationFactor; // 1250 Hz

		public List<double[]> GenerateSignals(IMicrophoneArray array, IAudioSource source, IEnvironmentSettings env)
		{
			var results = new List<double[]>();
			int samplesCount = (int)(_righSampleRate * _processingIntervalMs / 1000);
			double[] time = Enumerable.Range(0, samplesCount)
								   .Select(i => (double)i / _righSampleRate)
								   .ToArray();

			foreach (var mic in array.Microphones)
			{
				double[] signal = new double[samplesCount];
				float distance = ((MicrophoneArray)array).GetDistanceToSource((Microphone)mic);
				float delay = ((MicrophoneArray)array).GetDelayToSource((Microphone)mic);
				double attenuatedAmplitude = source.Amplitude / distance;

				for (int i = 0; i < samplesCount; i++)
				{
					double t = time[i] - delay;
					signal[i] = attenuatedAmplitude * Math.Sin(2 * Math.PI * source.Frequency * t + source.Phase);

					signal[i] += env.NoiseLevel * (new Random().NextDouble() - 0.5);
				}

				double[] decimated = DecimateSignal(signal, _decimationFactor);
				results.Add(decimated);
			}

			return results;
		}

		private double[] DecimateSignal(double[] input, int factor)
		{
			//double cutoff = (_righSampleRate / factor) / 2 * 0.8; // Частота среза с запасом 20%
			//var filtered = ApplyLowPassFilter(input, _righSampleRate, cutoff);

			// Простая децимация
			double[] output = new double[input.Length / factor];
			for (int i = 0; i < output.Length; i++)
			{
				output[i] = input[i * factor];
			}
			return output;
		}

		private double[] ApplyLowPassFilter(double[] input, double sampleRate, double cutoffFreq)
		{
			double rc = 1.0 / (2 * Math.PI * cutoffFreq);
			double dt = 1.0 / sampleRate;
			double alpha = dt / (rc + dt);

			double[] output = new double[input.Length];
			output[0] = input[0];

			for (int i = 1; i < input.Length; i++)
			{
				output[i] = output[i - 1] + alpha * (input[i] - output[i - 1]);
			}

			return output;
		}

	}
}