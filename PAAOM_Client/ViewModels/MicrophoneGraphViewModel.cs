using LiveCharts;
using LiveCharts.Wpf;
using PAAOM_Common;
using PAAOM_Common.Network.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PAAOM_Client
{
    public class MicrophoneGraphViewModel : INotifyPropertyChanged
    {
        private double[] _signalData;
        private string _packetInfo;
        private double _minAmplitude;
        private double _maxAmplitude;
        private RelayCommand _showSpectrumCommand;

        public int ChannelIndex { get; set; }
        public string Title { get; set; }

        public double TimeStep { get; } = 5.0;

        public Func<double, string> XAxisFormatter => value =>
            (value % TimeStep == 0) ? $"{value:F0}" : string.Empty;

        public Func<double, string> YAxisFormatter { get; } = value => $"{value:F2}";

        public SeriesCollection Series { get; } = new SeriesCollection
        {
            new LineSeries
            {
                Values = new ChartValues<double>(),
                LineSmoothness = 0,
                PointGeometry = null
            }
        };

        public ICommand ShowSpectrumCommand => _showSpectrumCommand;

        public string PacketInfo
        {
            get => _packetInfo;
            set
            {
                _packetInfo = value;
                OnPropertyChanged();
            }
        }

        public double MinAmplitude
        {
            get => _minAmplitude;
            set
            {
                _minAmplitude = value;
                OnPropertyChanged();
            }
        }

        public double MaxAmplitude
        {
            get => _maxAmplitude;
            set
            {
                _maxAmplitude = value;
                OnPropertyChanged();
            }
        }

        public MicrophoneGraphViewModel()
        {
            _showSpectrumCommand = new RelayCommand(ShowSpectrum, CanShowSpectrum);
        }

        public void UpdateSignalData(double[] signalData, AdcDataPacket packet)
        {
            Console.WriteLine($"[DEBUG] Channel {ChannelIndex}: Received {signalData?.Length ?? 0} samples");

            _signalData = signalData;

            Application.Current.Dispatcher.Invoke(() =>
            {
                Series[0].Values.Clear();
                foreach (var value in signalData)
                {
                    Series[0].Values.Add(value);
                }
            });

            PacketInfo = $"Пакет #{packet.SequenceNumber}, Время: {packet.StartTime}s, Fs: {Constants.SampleRate} Гц";
            CalculateAmplitudeRange();

            Console.WriteLine($"[DEBUG] Channel {ChannelIndex}: CanExecute = {CanShowSpectrum()}");

            _showSpectrumCommand.RaiseCanExecuteChanged();

            OnPropertyChanged(nameof(Series));
        }

        private void CalculateAmplitudeRange()
        {
            if (_signalData == null || _signalData.Length == 0)
            {
                MinAmplitude = -1;
                MaxAmplitude = 1;
                return;
            }

            double max = _signalData.Max();
            double min = _signalData.Min();

            double margin = Math.Max(Math.Abs(max), Math.Abs(min)) * 0.1;
            MaxAmplitude = max + margin;
            MinAmplitude = min - margin;
        }

        private void ShowSpectrum()
        {
            if (_signalData == null || _signalData.Length == 0) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var spectrumWindow = new SpectrumWindow(
                    _signalData,
                    Title,
                    Constants.SampleRate)
                {
                    Owner = Application.Current.MainWindow
                };
                spectrumWindow.Show();
            });
        }

        private bool CanShowSpectrum()
        {
            return _signalData != null && _signalData.Length > 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}