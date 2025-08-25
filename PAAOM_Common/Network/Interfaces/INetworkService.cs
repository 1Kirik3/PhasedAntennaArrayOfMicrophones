using PAAOM_Common.Network.Models;
using System;
using System.Net;
using System.Threading.Tasks;

namespace PAAOM_Common.Network.Interfaces
{
    internal interface INetworkService : IDisposable
    {
        event EventHandler<AvailabilityResponse> AvailabilityResponseReceived;
        event EventHandler<DetectionReport> DetectionReportReceived;
        event EventHandler<AdcDataPacket> AdcDataReceived;

        void Configure(IPEndPoint localEndpoint, IPEndPoint remoteEndpoint);
        Task<bool> SendAsync(PacketBase packet);
        Task<bool> CheckAvailabilityAsync(ushort packetId, int timeoutMs = 1000);
        void StartListening();
        void StopListening();
    }
}
