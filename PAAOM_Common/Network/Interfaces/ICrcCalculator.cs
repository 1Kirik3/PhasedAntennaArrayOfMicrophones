using System;

namespace PAAOM_Common.Network.Interfaces
{
    public interface ICrcCalculator
    {
        ushort ComputeChecksum(ReadOnlySpan<byte> bytes);
    }
}
