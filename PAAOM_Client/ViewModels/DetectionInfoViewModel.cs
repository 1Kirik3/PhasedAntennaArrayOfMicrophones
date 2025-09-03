using PAAOM_Common.Network.Models;
using System;
using System.ComponentModel;

namespace PAAOM_Client.ViewModels
{
    public class DetectionInfoViewModel : INotifyPropertyChanged
    {
        private string _detectionTime = "Нет данных";
        private string _measurementNumber = "Нет данных";
        private string _targetType = "Нет данных";
        private string _snrInfo = "Нет данных";
        private string _bearingInfo = "Нет данных";
        private string _bearingRateInfo = "Нет данных";
        private string _distanceInfo = "Нет данных";
        private string _distanceRateInfo = "Нет данных";
        private string _angleStdDevInfo = "Нет данных";
        private string _timeStdDevInfo = "Нет данных";

        public string DetectionTime
        {
            get => _detectionTime;
            set { _detectionTime = value; OnPropertyChanged(); }
        }

        public string MeasurementNumber
        {
            get => _measurementNumber;
            set { _measurementNumber = value; OnPropertyChanged(); }
        }

        public string TargetType
        {
            get => _targetType;
            set { _targetType = value; OnPropertyChanged(); }
        }

        public string SnrInfo
        {
            get => _snrInfo;
            set { _snrInfo = value; OnPropertyChanged(); }
        }

        public string BearingInfo
        {
            get => _bearingInfo;
            set { _bearingInfo = value; OnPropertyChanged(); }
        }

        public string BearingRateInfo
        {
            get => _bearingRateInfo;
            set { _bearingRateInfo = value; OnPropertyChanged(); }
        }

        public string DistanceInfo
        {
            get => _distanceInfo;
            set { _distanceInfo = value; OnPropertyChanged(); }
        }

        public string DistanceRateInfo
        {
            get => _distanceRateInfo;
            set { _distanceRateInfo = value; OnPropertyChanged(); }
        }

        public string AngleStdDevInfo
        {
            get => _angleStdDevInfo;
            set { _angleStdDevInfo = value; OnPropertyChanged(); }
        }

        public string TimeStdDevInfo
        {
            get => _timeStdDevInfo;
            set { _timeStdDevInfo = value; OnPropertyChanged(); }
        }

        public void UpdateFromReport(DetectionReport report)
        {
            var detectionTime = DateTimeOffset.FromUnixTimeSeconds(report.DetectionTimeUnix);
            DetectionTime = $"Время: {detectionTime:dd.MM.yyyy HH:mm:ss}.{report.DetectionTimeFine:D4}";

            MeasurementNumber = $"Измерение: #{report.MeasurementNumber}";

            TargetType = $"Тип: {GetTargetTypeDescription(report.TargetType)}";

            SnrInfo = $"ОСП: {report.Snr}";

            BearingInfo = $"Азимут: {report.Bearing:F1}°";

            BearingRateInfo = $"ВИП: {report.BearingRate:F1} °/с";

            DistanceInfo = $"Дистанция: {report.Distance} м";

            DistanceRateInfo = $"ВИР: {report.DistanceRate:F1} м/с";

            AngleStdDevInfo = $"СКО угла: {(report.AngleStdDev / 10.0):F1}°";

            TimeStdDevInfo = $"СКО времени: {(report.TimeStdDev / 100.0):F2} м";
        }

        private string GetTargetTypeDescription(ushort targetType)
        {
            return targetType switch
            {
                0 => "Неизвестно",
                1 => "Воздушная",
                2 => "Наземная",
                3 => "Морская",
                _ => $"Тип {targetType}"
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}