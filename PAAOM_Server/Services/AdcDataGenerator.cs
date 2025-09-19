using PAAOM_Common.Models;
using PAAOM_Common.Models.Interfaces;
using PAAOM_Common.Network.Models;
using System;

namespace PAAOM_Server.Services
{
    public class AdcDataGenerator
    {
        private readonly IEnvironment _environment;
        private readonly IAudioSource _audioSource;
        private readonly IMicrophoneArray _microphoneArray;
        private readonly SignalGenerator _signalGenerator;

        private ushort _sequenceNumber = 0;
        private static ushort _currentPacketId = 0;

        public AdcDataGenerator(IEnvironment environment, IAudioSource audioSource, IMicrophoneArray microphoneArray)
        {
            _environment = environment;
            _audioSource = audioSource;
            _microphoneArray = microphoneArray;
            _signalGenerator = new SignalGenerator();
        }

        public AdcDataPacket GenerateAdcData(uint startTime = 0)
        {
            var signals = _signalGenerator.GenerateSignals(_microphoneArray, _audioSource, _environment);

            var packet = new AdcDataPacket
            {
                PacketId = _currentPacketId++,
                SequenceNumber = _sequenceNumber++,
                StartTime = startTime > 0 ? startTime : (uint)(DateTime.Now.TimeOfDay.TotalSeconds),
                Reserved = 0
            };

            // Для SampleRate = 1250 Гц генерируем 125 отсчетов
            for (int channel = 0; channel < Math.Min(8, signals.Count); channel++)
            {
                if (signals[channel].Length != 125)
                {
                    throw new InvalidOperationException(
                        $"Неверное количество отсчетов: {signals[channel].Length}, ожидалось 125");
                }

                for (int sample = 0; sample < 125; sample++)
                {
                    double sampleValue = signals[channel][sample];
                    sampleValue = Math.Max(-1.0, Math.Min(1.0, sampleValue));
                    packet.ChannelSamples[channel][sample] = (short)(sampleValue * short.MaxValue);
                }
            }

            return packet;
        }
    }
}