using System;
using System.Buffers.Binary;

namespace PAAOM_Common.Network.Models
{
    public class AvailabilityRequest : PacketBase
    {
        public override byte Type => 0x02;
        public override ushort CalculateBodyLength() => 2;
        public override void WriteBody(Span<byte> buffer)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(buffer, PacketId);
        }

        public static AvailabilityRequest ReadBody(ReadOnlySpan<byte> bodyBuffer)
        {
            return new AvailabilityRequest
            {
                PacketId = BinaryPrimitives.ReadUInt16LittleEndian(bodyBuffer)
            };
        }
    }
}
