using PAAOM_Common.Models;
using PAAOM_Common.Models.Interfaces;
using PAAOM_Common.Network.Models;

namespace PAAOM_Server.Services
{
    public class DetectionCalculator
    {
        private readonly IEnvironmentSettings _environment;
        private readonly IAudioSource _audioSource;
        private readonly IMicrophoneArray _microphoneArray;
        private readonly SignalGenerator _signalGenerator;

        private uint _measurementCounter = 0;
        private DateTime _lastDetectionTime = DateTime.MinValue;
        private float _lastBearing = 0;
        private float _lastDistance = 0;

        public DetectionCalculator(IEnvironmentSettings environment, IAudioSource audioSource, IMicrophoneArray microphoneArray)
        {
            _environment = environment;
            _audioSource = audioSource;
            _microphoneArray = microphoneArray;
            _signalGenerator = new SignalGenerator();
        }

        public DetectionReport CalculateDetectionReport()
        {
            var signals = _signalGenerator.GenerateSignals(_microphoneArray, _audioSource, _environment);

            var bearing = CalculateBearing();
            var distance = CalculateDistance();
            var snr = CalculateSNR(signals);

            var bearingRate = CalculateBearingRate(bearing);
            var distanceRate = CalculateDistanceRate(distance);

            var angleStdDev = CalculateAngleStdDev(bearing);
            var timeStdDev = CalculateTimeStdDev(distance);

            var currentTime = DateTime.UtcNow;

            var report = new DetectionReport
            {
                DetectionTimeUnix = (uint)new DateTimeOffset(currentTime).ToUnixTimeSeconds(),
                DetectionTimeFine = (ushort)(currentTime.Millisecond * 10), // десятые доли миллисекунд
                MeasurementNumber = (ushort)(++_measurementCounter),
                TargetType = DetermineTargetType(),
                Snr = (ushort)snr,
                Bearing = bearing,
                BearingRate = bearingRate,
                Distance = (ushort)distance,
                DistanceRate = distanceRate,
                AngleStdDev = (ushort)(angleStdDev * 10), // в десятых долях градуса
                TimeStdDev = (ushort)(timeStdDev * 100)   // в сантиметрах
            };

            _lastDetectionTime = currentTime;
            _lastBearing = bearing;
            _lastDistance = distance;

            return report;
        }

        private float CalculateBearing()
        {
            var center = _microphoneArray.ArrayCenter;
            var sourcePos = _audioSource.Position;

            double dx = sourcePos.X - center.X;
            double dy = sourcePos.Y - center.Y;

            double bearingRad = Math.Atan2(dy, dx);
            double bearingDeg = bearingRad * 180 / Math.PI;

            if (bearingDeg < 0) bearingDeg += 360;

            return (float)Math.Round(bearingDeg, 1);
        }

        private float CalculateDistance()
        {
            var center = _microphoneArray.ArrayCenter;
            var sourcePos = _audioSource.Position;

            double dx = sourcePos.X - center.X;
            double dy = sourcePos.Y - center.Y;
            double dz = sourcePos.Z - center.Z;

            double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);

            return (float)Math.Round(Math.Max(100, Math.Min(5000, distance)));
        }

        private float CalculateSNR(List<double[]> signals)
        {

            double signalPower = signals.Average(channel =>
                channel.Select(s => s * s).Average());

            double noisePower = _environment.NoiseLevel * _environment.NoiseLevel;

            double snr = 10 * Math.Log10(signalPower / (noisePower + 1e-10));

            return (float)Math.Max(0, Math.Min(65535, snr + 30));
        }

        private float CalculateBearingRate(float currentBearing)
        {
            if (_lastDetectionTime == DateTime.MinValue)
                return 0f;

            double timeDiff = (DateTime.UtcNow - _lastDetectionTime).TotalSeconds;
            if (timeDiff < 0.1) return 0f;

            double bearingDiff = currentBearing - _lastBearing;
            if (bearingDiff > 180) bearingDiff -= 360;
            if (bearingDiff < -180) bearingDiff += 360;

            double bearingRate = bearingDiff / timeDiff;

            return (float)Math.Round(Math.Max(0, Math.Min(20, Math.Abs(bearingRate))) * Math.Sign(bearingRate), 1);
        }

        private float CalculateDistanceRate(float currentDistance)
        {
            if (_lastDetectionTime == DateTime.MinValue)
                return 0f;

            double timeDiff = (DateTime.UtcNow - _lastDetectionTime).TotalSeconds;

            if (timeDiff < 0.5) return 0f; // Ждем 0.5 секунды

            double distanceDiff = currentDistance - _lastDistance;
            double distanceRate = distanceDiff / timeDiff;

            distanceRate = Math.Max(-30, Math.Min(30, distanceRate));

            return (float)Math.Round(distanceRate, 1); // шаг 0.5 м/с
        }

        private float CalculateAngleStdDev(float bearing)
        {
            double snrFactor = 1.0 / (1 + Math.Exp(-(Snr - 20) / 5));
            double baseStdDev = 5.0; // градусов при плохом ОСП
            double minStdDev = 0.5;  // градусов при хорошем ОСП

            double angleStdDev = minStdDev + (baseStdDev - minStdDev) * (1 - snrFactor);

            return (float)Math.Round(angleStdDev, 1);
        }

        private float CalculateTimeStdDev(float distance)
        {
            double distanceFactor = distance / 1000.0; // нормировка
            double snrFactor = 1.0 / (1 + Math.Exp(-(Snr - 15) / 3));

            double timeStdDev = 0.1 + 0.4 * distanceFactor * (1 - snrFactor); // метры

            return (float)Math.Round(timeStdDev, 2);
        }

        private ushort DetermineTargetType()
        {
            var sourcePos = _audioSource.Position;
            var arrayCenter = _microphoneArray.ArrayCenter;

            double heightDifference = sourcePos.Z - arrayCenter.Z;

            if (heightDifference > 2)
                return 1; // Воздушная
            else if (heightDifference < -2)
                return 3; // Морская
            else if (Math.Abs(heightDifference) <= 2)
                return 2; // Наземная

            return 0; // Незивестно
        }

        private float Snr { get; set; }
    }
}