using PAAOM_Common.Network.Models;
using System;

namespace PAAOM_Common.Network.Interfaces
{
    internal interface IPacketBuilder
    {
        byte[] BuildPacket(PacketBase packet);
        bool TryParsePacket(byte[] data, out PacketBase packet);
        bool TryParsePacket(ArraySegment<byte> data, out PacketBase packet);
    }
}
