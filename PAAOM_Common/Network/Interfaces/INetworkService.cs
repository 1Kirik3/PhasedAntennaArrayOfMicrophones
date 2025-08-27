using PAAOM_Common.Network.Models;
using System;
using System.Net;
using System.Threading.Tasks;

namespace PAAOM_Common.Network.Interfaces
{
    public interface INetworkService : IDisposable
    {
        event EventHandler<AvailabilityResponse> AvailabilityResponseReceived;
        event EventHandler<DetectionReport> DetectionReportReceived;
        event EventHandler<AdcDataPacket> AdcDataReceived;

        bool IsListening { get; }

        void Configure(IPEndPoint localEndpoint, IPEndPoint remoteEndpoint);
        Task<bool> SendAsync(PacketBase packet);
        Task<bool> CheckAvailabilityAsync(ushort packetId, int timeoutMs = 1000);
        void StartListening();
        void StopListening();
    }
}
