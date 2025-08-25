using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using MathNet.Numerics.IntegralTransforms;
using System.ComponentModel;
using System.Numerics;
using System.Windows;

namespace PAAOM_Server
{
	public partial class SpectrumWindow : Window
	{
		public ChartValues<double> SpectrumValues { get; set; }
		public List<string> FrequencyLabels { get; set; }
		public double MaxAmplitude { get; private set; }
		private double _sampleRate;
		private double _inputSignalFrequency; // Частота исходного сигнала

		public Func<double, string> XAxisFormatter { get; set; }
		public Func<double, string> YAxisFormatter { get; set; }

		public SpectrumWindow(double[] signal, string microphoneName, double sampleRate, double inputFrequency)
		{
			InitializeComponent();
			Title = $"Спектр - {microphoneName}";
			_sampleRate = sampleRate;
			_inputSignalFrequency = inputFrequency; // Сохраняем частоту сигнала

			var calculator = new SpectrumCalculator();
			var (frequencies, magnitudes) = calculator.CalculateSpectrum(signal, sampleRate);

			SpectrumValues = new ChartValues<double>(magnitudes);
			FrequencyLabels = frequencies.Select(f => $"{f:F1}").ToList();
			MaxAmplitude = magnitudes.DefaultIfEmpty(0).Max() * 1.1;

			DataContext = this;
			ConfigureChart();
			this.Closing += (s, e) => this.Owner?.Activate();
		}

		private void ConfigureChart()
		{
			if (SpectrumChart == null || !FrequencyLabels.Any()) return;

			double[] freqValues = FrequencyLabels.Select(f => double.Parse(f)).ToArray();
			double peakFreq = FindPeakFrequency(freqValues, SpectrumValues.ToArray());

			double margin = peakFreq * 0.1;
			double minFreq = Math.Max(0, peakFreq - margin);
			double maxFreq = peakFreq + margin;

			double absoluteMaxFreq = freqValues.Last();
			if (maxFreq > absoluteMaxFreq)
			{
				maxFreq = absoluteMaxFreq;
				minFreq = Math.Max(0, maxFreq - 2 * margin);
			}

			UpdatePeakInfo(peakFreq);

			SpectrumChart.AxisX.Clear();
			SpectrumChart.AxisX.Add(new Axis
			{
				Title = "Частота (Гц)",
				MinValue = minFreq,
				MaxValue = maxFreq,
				LabelFormatter = value => $"{value:F0}",
				Separator = new Separator { Step = (maxFreq - minFreq) / 10 }
			});

			SpectrumChart.Series.Clear();
			SpectrumChart.Series.Add(new LineSeries
			{
				Values = new ChartValues<double>(SpectrumValues),
				PointGeometry = null,
				StrokeThickness = 2,
				Configuration = new CartesianMapper<double>()
					.X((value, index) => freqValues[index])
					.Y(value => value)
			});
		}

		private void UpdatePeakInfo(double displayedPeakFreq)
		{
			// Всегда показываем реальную частоту, если она известна
			string infoText = $"Пик: {displayedPeakFreq:F0} Гц";

			if (_inputSignalFrequency > _sampleRate / 2)
			{
				infoText += $" (реальная частота: {_inputSignalFrequency:F0} Гц) [эффект наложения]";
			}
			else if (Math.Abs(_inputSignalFrequency - displayedPeakFreq) > 1)
			{
				infoText += $" (реальная частота: {_inputSignalFrequency:F0} Гц)";
			}

			TbPeakInfo.Text = infoText;
		}

		private double FindPeakFrequency(double[] frequencies, double[] magnitudes)
		{
			int peakIndex = 0;
			double maxMagnitude = 0;

			for (int i = 0; i < magnitudes.Length; i++)
			{
				if (magnitudes[i] > maxMagnitude)
				{
					maxMagnitude = magnitudes[i];
					peakIndex = i;
				}
			}

			return frequencies[peakIndex];
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			Owner?.Activate();
		}

		private void AutoScale_Click(object sender, RoutedEventArgs e)
		{
			ConfigureChart();
		}
	}

	public class SpectrumCalculator
	{
		public (double[] frequencies, double[] magnitudes) CalculateSpectrum(double[] signal, double sampleRate)
		{
			int n = signal.Length;

			// Window function
			double[] window = MathNet.Numerics.Window.Hann(n);
			double windowGain = window.Sum() / n;
			Complex[] complexSignal = new Complex[n];

			for (int i = 0; i < n; i++)
			{
				complexSignal[i] = new Complex(signal[i] * window[i] / windowGain, 0);
			}

			// FFT
			Fourier.Forward(complexSignal, FourierOptions.Default);

			// Amplitude spectrum
			int spectrumLength = n / 2;
			double[] frequencies = new double[spectrumLength];
			double[] magnitudes = new double[spectrumLength];

			for (int i = 0; i < spectrumLength; i++)
			{
				frequencies[i] = i * sampleRate / n;
				if (frequencies[i] > sampleRate / 2)
					frequencies[i] = sampleRate - frequencies[i];

				magnitudes[i] = complexSignal[i].Magnitude * 2 / n;
			}

			return (frequencies, magnitudes);
		}
	}
}