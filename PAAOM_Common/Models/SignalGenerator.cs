using PAAOM_Common.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PAAOM_Common.Models
{
    public class SignalGenerator
    {
        private const float _highSampleRate = 10000f;    // 10 kHz
        private const int _decimationFactor = 8;
        private const float _processingIntervalMs = 100f;
        private readonly Random _random = new Random();

        public const double OutputSampleRate = _highSampleRate / _decimationFactor; // 1250 Hz
        public const int OutputSamplesCount = 125; // 100ms при 1250 Hz

        public List<double[]> GenerateSignals(IMicrophoneArray array, IAudioSource source, IEnvironmentSettings env)
        {
            var results = new List<double[]>();

            // Отсчеты до децимации
            int inputSamplesCount = (int)(_highSampleRate * _processingIntervalMs / 1000);
            double[] time = Enumerable.Range(0, inputSamplesCount)
                                   .Select(i => (double)i / _highSampleRate)
                                   .ToArray();

            // Генерация общего шума для всех каналов
            double[] commonNoise = GenerateNoise(inputSamplesCount, env.NoiseLevel);

            foreach (var mic in array.Microphones)
            {
                float distance = ((MicrophoneArray)array).GetDistanceToSource((Microphone)mic);
                float delay = ((MicrophoneArray)array).GetDelayToSource((Microphone)mic);
                double attenuatedAmplitude = source.Amplitude / Math.Max(0.1, distance);

                // Генерация сигнала с высокой частотой дискретизации
                double[] highRateSignal = new double[inputSamplesCount];
                for (int i = 0; i < inputSamplesCount; i++)
                {
                    double delayedTime = time[i] + delay;
                    highRateSignal[i] = attenuatedAmplitude *
                        Math.Sin(2 * Math.PI * source.Frequency * delayedTime + source.Phase);
                    highRateSignal[i] += commonNoise[i];
                }

                // НЧ фильтр + децимация
                double[] filtered = ApplyLowPassFilter(highRateSignal, _highSampleRate, OutputSampleRate / 2 * 0.8);
                double[] decimated = DecimateSignal(filtered, _decimationFactor);

                if (decimated.Length != OutputSamplesCount)
                {
                    decimated = AdjustOutputLength(decimated, OutputSamplesCount);
                }

                results.Add(decimated);
            }

            return results;
        }

        private double[] AdjustOutputLength(double[] signal, int targetLength)
        {
            if (signal.Length == targetLength) return signal;

            double[] result = new double[targetLength];
            int copyLength = Math.Min(signal.Length, targetLength);

            Array.Copy(signal, result, copyLength);

            // Дозаполнение нулями
            for (int i = copyLength; i < targetLength; i++)
            {
                result[i] = 0;
            }

            return result;
        }

        private double[] GenerateNoise(int length, float noiseLevel)
        {
            double[] noise = new double[length];
            for (int i = 0; i < length; i++)
            {
                noise[i] = noiseLevel * (_random.NextDouble() - 0.5);
            }
            return noise;
        }

        private double[] DecimateSignal(double[] input, int factor)
        {
            int outputLength = input.Length / factor;
            double[] output = new double[outputLength];

            for (int i = 0; i < outputLength; i++)
            {
                output[i] = input[i * factor];
            }

            return output;
        }

        private double[] ApplyLowPassFilter(double[] input, double sampleRate, double cutoffFreq)
        {
            // Упрощенный RC-фильтр
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

        public List<double[]> GenerateSignalsWithValidation(IMicrophoneArray array, IAudioSource source, IEnvironmentSettings env)
        {
            if (source.Frequency > OutputSampleRate / 2)
            {
                throw new ArgumentException(
                    $"Частота сигнала ({source.Frequency} Hz) превышает частоту Найквиста ({OutputSampleRate / 2} Hz)");
            }

            return GenerateSignals(array, source, env);
        }
    }
}