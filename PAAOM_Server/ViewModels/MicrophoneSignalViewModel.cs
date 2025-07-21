
using LiveCharts;
using LiveCharts.Wpf;

namespace PAAOM_Server.ViewModels
{
    class MicrophoneSignalViewModel
    {
		public int MicrophoneIndex { get; }
		public SeriesCollection ChartSeries { get; }
		public ChartValues<double> Values { get; }

		public MicrophoneSignalViewModel(int index, double[] samples)
		{
			MicrophoneIndex = index;
			Values = new ChartValues<double>(samples);

			ChartSeries = new SeriesCollection
		{
			new LineSeries
			{
				Title = $"Mic {index + 1}",
				Values = Values,
				PointGeometrySize = 0,
				LineSmoothness = 0
			}
		};
		}
	}
}
