using System;
using System.Buffers.Binary;

namespace PAAOM_Common.Network.Models
{
    public class AdcDataPacket : PacketBase
    {
        public override byte Type => 0x06;

        public ushort SequenceNumber { get; set; }           // 2 байта
        public uint StartTime { get; set; }                  // 4 байта (секунды с 0:00)
        public ushort Reserved { get; set; } = 0;            // 2 байта
        public short[][] ChannelSamples { get; set; }        // 8 каналов, по 125 отсчетов (short)

        public AdcDataPacket()
        {
            // Инициализируем массив для 8 каналов
            ChannelSamples = new short[8][];
            for (int i = 0; i < 8; i++)
            {
                ChannelSamples[i] = new short[125]; // По 125 отсчетов на канал
            }
        }

        public override ushort CalculateBodyLength() => (ushort)(2 + 4 + 2 + (8 * 125 * 2)); // 2008

        public override void WriteBody(Span<byte> buffer)
        {
            int offset = 0;
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(offset, 2), SequenceNumber);
            offset += 2;
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(offset, 4), StartTime);
            offset += 4;
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(offset, 2), Reserved);
            offset += 2;

            // Записываем отсчеты всех каналов
            for (int channel = 0; channel < 8; channel++)
            {
                for (int sample = 0; sample < 125; sample++)
                {
                    BinaryPrimitives.WriteInt16LittleEndian(buffer.Slice(offset, 2), ChannelSamples[channel][sample]);
                    offset += 2;
                }
            }
        }

        public static AdcDataPacket ReadBody(ReadOnlySpan<byte> bodyBuffer)
        {
            int offset = 0;
            var packet = new AdcDataPacket();

            packet.SequenceNumber = BinaryPrimitives.ReadUInt16LittleEndian(bodyBuffer.Slice(offset, 2));
            offset += 2;
            packet.StartTime = BinaryPrimitives.ReadUInt32LittleEndian(bodyBuffer.Slice(offset, 4));
            offset += 4;
            packet.Reserved = BinaryPrimitives.ReadUInt16LittleEndian(bodyBuffer.Slice(offset, 2));
            offset += 2;

            // Читаем отсчеты всех каналов
            for (int channel = 0; channel < 8; channel++)
            {
                for (int sample = 0; sample < 125; sample++)
                {
                    packet.ChannelSamples[channel][sample] = BinaryPrimitives.ReadInt16LittleEndian(bodyBuffer.Slice(offset, 2));
                    offset += 2;
                }
            }

            return packet;
        }
    }
}
