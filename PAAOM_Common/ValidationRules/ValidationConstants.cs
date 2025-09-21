using System;
using System.Collections.Generic;
using System.Linq;

namespace PAAOM_Common.ValidationRules
{
    public static class ValidationConstants
    {
        public const double MaxFrequency = 625.0;

        public const double MinTemperature = -100.0;
        public const double MaxTemperature = 100.0;

        public const double MinNoiseLevel = 0.0;
        public const double MaxNoiseLevel = 1.0;

        public const double MinCoordinate = -1000.0;
        public const double MaxCoordinate = 1000.0;

        public const double MinAmplitude = 0.0;
        public const double MaxAmplitude = 10.0;

        public const double MinPhase = -Math.PI;
        public const double MaxPhase = Math.PI;

        public const double MinRadius = 0.1;
        public const double MaxRadius = 10.0;

        public const double MinSendingInterval = 10.0;
        public const double MaxSendingInterval = 10000.0;
    }
}
