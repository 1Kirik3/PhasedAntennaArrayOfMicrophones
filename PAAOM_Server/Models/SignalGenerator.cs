using PAAOM_Server.Models.Interfaces;

namespace PAAOM_Server.Models
{
	public class SignalGenerator
	{
		private const float _righSampleRate = 10000f;
		private const int _decimationFactor = 8;
		private const float _processingIntervalMs = 100f;

		public const double OutputSampleRate = _righSampleRate / _decimationFactor; // 1250 Гц

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
			int outputLength = input.Length / factor;
			double[] output = new double[outputLength];

			for (int i = 0; i < outputLength; i++)
			{
				double sum = 0;
				for (int j = 0; j < factor; j++)
				{
					sum += input[i * factor + j];
				}
				output[i] = sum / factor;
			}
			return output;
		}
	}
}