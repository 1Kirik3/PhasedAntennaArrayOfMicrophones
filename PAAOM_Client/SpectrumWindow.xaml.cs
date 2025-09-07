using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using System.ComponentModel;
using System.Windows;

namespace PAAOM_Client
{
    public partial class SpectrumWindow : Window
    {
        public ChartValues<double> SpectrumValues { get; set; }
        public double[] Frequencies { get; set; }
        public double MaxAmplitude { get; private set; }
        private double _sampleRate;

        public Func<double, string> XAxisFormatter => value => $"{value:F0}";
        public Func<double, string> YAxisFormatter => value => $"{value:F2}";

        public SpectrumWindow(double[] signal, string channelName, double sampleRate)
        {
            InitializeComponent();
            Title = $"Спектр - {channelName}";
            _sampleRate = sampleRate;

            CalculateAndDisplaySpectrum(signal);
        }

        private void CalculateAndDisplaySpectrum(double[] signal)
        {
            var calculator = new SpectrumCalculator();
            var (frequencies, magnitudes) = calculator.CalculateSpectrum(signal, _sampleRate);

            Frequencies = frequencies;
            SpectrumValues = new ChartValues<double>(magnitudes);
            MaxAmplitude = magnitudes.DefaultIfEmpty(0).Max() * 1.1;

            var mapper = Mappers.Xy<double>()
                .X((value, index) => Frequencies[index])
                .Y(value => value);

            var series = new LineSeries
            {
                Title = "Амплитудный спектр",
                Values = SpectrumValues,
                PointGeometry = null,
                StrokeThickness = 2,
                Stroke = System.Windows.Media.Brushes.RoyalBlue,
                Fill = System.Windows.Media.Brushes.Transparent,
                Configuration = mapper
            };

            SpectrumChart.Series.Clear();
            SpectrumChart.Series.Add(series);

            DataContext = this;
            FindAndDisplayPeak();
        }

        private void FindAndDisplayPeak()
        {
            if (SpectrumValues == null || !SpectrumValues.Any()) return;

            int peakIndex = 1;
            for (int i = 2; i < SpectrumValues.Count; i++)
            {
                if (SpectrumValues[i] > SpectrumValues[peakIndex])
                {
                    peakIndex = i;
                }
            }

            double peakFrequency = Frequencies[peakIndex];
            double peakAmplitude = SpectrumValues[peakIndex];

            PeakInfo.Text = $"Пик: {peakFrequency:F0} Гц ({peakAmplitude:F2})";
            FrequencyRange.Text = $"Диапазон: {Frequencies[1]:F0} - {Frequencies.Last():F0} Гц | Fs: {_sampleRate} Гц";

            double margin = Math.Max(50, peakFrequency * 0.2);
            SpectrumChart.AxisX[0].MinValue = Math.Max(0, peakFrequency - margin);
            SpectrumChart.AxisX[0].MaxValue = Math.Min(Frequencies.Last(), peakFrequency + margin);
            SpectrumChart.AxisY[0].MinValue = 0;
            SpectrumChart.AxisY[0].MaxValue = MaxAmplitude;
        }

        private void AutoScale_Click(object sender, RoutedEventArgs e)
        {
            if (Frequencies != null && Frequencies.Length > 1)
            {
                SpectrumChart.AxisX[0].MinValue = Frequencies[1];
                SpectrumChart.AxisX[0].MaxValue = Frequencies.Last();
            }
            SpectrumChart.AxisY[0].MinValue = 0;
            SpectrumChart.AxisY[0].MaxValue = MaxAmplitude;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Owner?.Activate();
        }
    }
}