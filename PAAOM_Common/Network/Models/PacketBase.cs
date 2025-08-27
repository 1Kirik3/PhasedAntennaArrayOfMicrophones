using System;

namespace PAAOM_Common.Network.Models
{
    public abstract class PacketBase
    {
        public const ushort MagicWord = 0xDFAD; // 0xAD 0xDF в Little-Endian
        public abstract byte Type { get; }
        public ushort PacketId { get; set; }
        public abstract ushort CalculateBodyLength();
        public abstract void WriteBody(Span<byte> buffer);
    }
}
