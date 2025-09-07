using MathNet.Numerics.IntegralTransforms;
using System.Numerics;

namespace PAAOM_Client
{
    public class SpectrumCalculator
    {
        public (double[] frequencies, double[] magnitudes) CalculateSpectrum(double[] signal, double sampleRate)
        {
            int n = signal.Length;

            double[] window = MathNet.Numerics.Window.Hann(n);
            double windowGain = window.Sum() / n; // Среднее значение окна для компенсации амплитуды
            Complex[] complexSignal = new Complex[n];

            for (int i = 0; i < n; i++)
            {
                complexSignal[i] = new Complex(signal[i] * window[i] / windowGain, 0);
            }

            Fourier.Forward(complexSignal, FourierOptions.Matlab);

            int spectrumLength = (n % 2 == 0) ? (n / 2 + 1) : ((n + 1) / 2);
            double[] frequencies = new double[spectrumLength];
            double[] magnitudes = new double[spectrumLength];

            for (int i = 0; i < spectrumLength; i++)
            {
                frequencies[i] = i * sampleRate / n;
                magnitudes[i] = complexSignal[i].Magnitude;
                if (i > 0 && i != spectrumLength - 1)
                {
                    magnitudes[i] *= 2;
                }
                magnitudes[i] /= n;
            }

            return (frequencies, magnitudes);
        }
    }

}