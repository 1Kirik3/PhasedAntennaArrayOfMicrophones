using System;
using System.Buffers.Binary;

namespace PAAOM_Common.Network.Models
{
    public class AvailabilityResponse : PacketBase
    {
        public override byte Type => 0x03;
        public override ushort CalculateBodyLength() => 2;
        public override void WriteBody(Span<byte> buffer)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(buffer, PacketId);
        }

        public static AvailabilityResponse ReadBody(ReadOnlySpan<byte> bodyBuffer)
        {
            return new AvailabilityResponse
            {
                PacketId = BinaryPrimitives.ReadUInt16LittleEndian(bodyBuffer)
            };
        }
    }
}
