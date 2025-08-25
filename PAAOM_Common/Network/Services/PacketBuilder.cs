using PAAOM_Common.Network.Interfaces;
using PAAOM_Common.Network.Models;
using System;

namespace PAAOM_Common.Network.Services
{
    public class PacketBuilder : IPacketBuilder
    {
        private readonly ICrcCalculator _crcCalculator;

        public PacketBuilder(ICrcCalculator crcCalculator)
        {
            _crcCalculator = crcCalculator ?? throw new ArgumentNullException(nameof(crcCalculator));
        }

        public byte[] BuildPacket(PacketBase packet)
        {
            ushort bodyLength = packet.CalculateBodyLength();
            int totalLength = 6 + bodyLength + 2;
            byte[] packetData = new byte[totalLength];

            var buffer = new ArraySegment<byte>(packetData, 0, totalLength);

            WriteUInt16LittleEndian(buffer, 0, PacketBase.MagicWord);
            packetData[2] = packet.Type;
            packetData[3] = 0;
            WriteUInt16LittleEndian(buffer, 4, bodyLength);

            packet.WriteBody(new ArraySegment<byte>(packetData, 6, bodyLength));

            ushort crc = _crcCalculator.ComputeChecksum(new ArraySegment<byte>(packetData, 0, 6 + bodyLength));
            WriteUInt16LittleEndian(buffer, 6 + bodyLength, crc);

            return packetData;
        }

        public bool TryParsePacket(byte[] data, out PacketBase packet)
        {
            return TryParsePacket(new ArraySegment<byte>(data), out packet);
        }

        public bool TryParsePacket(ArraySegment<byte> data, out PacketBase packet)
        {
            packet = null;

            if (data.Count < 8)
                return false;

            ushort magic = ReadUInt16LittleEndian(data, 0);
            if (magic != PacketBase.MagicWord)
                return false;

            ushort bodyLength = ReadUInt16LittleEndian(data, 4);

            int expectedPacketLength = 6 + bodyLength + 2;
            if (data.Count < expectedPacketLength)
                return false;

            ushort receivedCrc = ReadUInt16LittleEndian(data, 6 + bodyLength);
            ushort calculatedCrc = _crcCalculator.ComputeChecksum(new ArraySegment<byte>(data.Array, data.Offset, 6 + bodyLength));
            if (receivedCrc != calculatedCrc)
                return false;

            var bodyBuffer = new ArraySegment<byte>(data.Array, data.Offset + 6, bodyLength);
            byte packetType = data.Array[data.Offset + 2];

            try
            {
                switch (packetType)
                {
                    case 0x02:
                        packet = AvailabilityRequest.ReadBody(bodyBuffer);
                        break;
                    case 0x03:
                        packet = AvailabilityResponse.ReadBody(bodyBuffer);
                        break;
                    case 0x04:
                        packet = DetectionReport.ReadBody(bodyBuffer);
                        break;
                    case 0x06:
                        packet = AdcDataPacket.ReadBody(bodyBuffer);
                        break;
                    default:
                        return false;
                }
                return packet != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static ushort ReadUInt16LittleEndian(ArraySegment<byte> buffer, int offset)
        {
            byte[] array = buffer.Array;
            int index = buffer.Offset + offset;
            return (ushort)(array[index] | (array[index + 1] << 8));
        }

        private static void WriteUInt16LittleEndian(ArraySegment<byte> buffer, int offset, ushort value)
        {
            byte[] array = buffer.Array;
            int index = buffer.Offset + offset;
            array[index] = (byte)(value & 0xFF);
            array[index + 1] = (byte)((value >> 8) & 0xFF);
        }

    }
}
