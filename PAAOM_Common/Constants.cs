
namespace PAAOM_Common
{
    public static class Constants
    {
        public const int SampleRate = 1250; // Гц
        public const int SamplesPerPacket = 125; 
        public const double PacketDurationMs = (double)SamplesPerPacket / SampleRate * 1000; // 100 мс
    }
}
