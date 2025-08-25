using PAAOM_Common.Network.Interfaces;
using System;

namespace PAAOM_Common.Network.Services
{
    public class Crc16Calculator : ICrcCalculator
    {
        private const ushort Polynomial = 0xA001;
        private static readonly ushort[] Table = new ushort[256];

        static Crc16Calculator()
        {
            for (ushort i = 0; i < Table.Length; ++i)
            {
                ushort value = 0;
                ushort temp = i;
                for (byte j = 0; j < 8; ++j)
                {
                    if (((value ^ temp) & 0x0001) != 0)
                    {
                        value = (ushort)((value >> 1) ^ Polynomial);
                    }
                    else
                    {
                        value >>= 1;
                    }
                    temp >>= 1;
                }
                Table[i] = value;
            }
        }

        public ushort ComputeChecksum(ReadOnlySpan<byte> bytes)
        {
            ushort crc = 0;
            for (int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(crc ^ bytes[i]);
                crc = (ushort)((crc >> 8) ^ Table[index]);
            }
            return crc;
        }
    }
}
