using System;

namespace PAAOM_Common.Network.Interfaces
{
    internal interface ICrcCalculator
    {
        ushort ComputeChecksum(ReadOnlySpan<byte> bytes);
    }
}
