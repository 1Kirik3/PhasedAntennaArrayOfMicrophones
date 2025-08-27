using PAAOM_Common.Network.Interfaces;
using PAAOM_Common.Network.Models;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PAAOM_Common.Network.Services
{
    public class UdpNetworkService : INetworkService
    {
        private UdpClient _udpClient;
        private readonly IPacketBuilder _packetBuilder;
        private bool _isListening = false;
        private IPEndPoint _remoteEndPoint;

        public bool IsListening => _isListening;

        public event EventHandler<AvailabilityResponse> AvailabilityResponseReceived;
        public event EventHandler<DetectionReport> DetectionReportReceived;
        public event EventHandler<AdcDataPacket> AdcDataReceived;

        public UdpNetworkService(IPacketBuilder packetBuilder)
        {
            _packetBuilder = packetBuilder;
            _udpClient = new UdpClient();
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        public void Configure(IPEndPoint localEndpoint, IPEndPoint remoteEndpoint)
        {
            try
            {
                // Закрываем предыдущее соединение если было
                _udpClient?.Close();
                _udpClient = new UdpClient();
                _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpClient.Client.Bind(localEndpoint);
                _remoteEndPoint = remoteEndpoint;
            }
            catch (SocketException ex)
            {
                throw new Exception($"Не удалось занять порт {localEndpoint.Port}: {ex.Message}", ex);
            }
        }

        public void StartListening()
        {
            if (_isListening) return;
            _isListening = true;
            _ = Task.Run(ListenLoop);
        }

        public void StopListening()
        {
            _isListening = false;
            _udpClient?.Close();
        }

        public async Task<bool> SendAsync(PacketBase packet)
        {
            if (_remoteEndPoint == null)
                throw new InvalidOperationException("Remote endpoint is not configured.");

            try
            {
                byte[] data = _packetBuilder.BuildPacket(packet);
                await _udpClient.SendAsync(data, data.Length, _remoteEndPoint);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> CheckAvailabilityAsync(ushort packetId, int timeoutMs = 1000)
        {
            var request = new AvailabilityRequest { PacketId = packetId };
            var responseTask = WaitForSpecificResponse<AvailabilityResponse>(
                timeoutMs, resp => resp.PacketId == packetId);

            await SendAsync(request);
            var response = await responseTask;
            return response != null;
        }

        private async Task ListenLoop()
        {
            while (_isListening)
            {
                try
                {
                    UdpReceiveResult result = await _udpClient.ReceiveAsync();
                    if (_packetBuilder.TryParsePacket(result.Buffer, out var packet))
                    {
                        HandleReceivedPacket(packet, result.RemoteEndPoint);
                    }
                }
                catch (ObjectDisposedException) { break; }
                catch (Exception)
                {
                    await Task.Delay(1000);
                }
            }
        }

        private void HandleReceivedPacket(PacketBase packet, IPEndPoint sender)
        {
            switch (packet)
            {
                case AvailabilityResponse resp:
                    AvailabilityResponseReceived?.Invoke(this, resp);
                    break;
                case DetectionReport report:
                    DetectionReportReceived?.Invoke(this, report);
                    break;
                case AdcDataPacket adcData:
                    AdcDataReceived?.Invoke(this, adcData);
                    break;
            }
        }

        private Task<T> WaitForSpecificResponse<T>(int timeoutMs, Func<T, bool> predicate) where T : PacketBase
        {
            var tcs = new TaskCompletionSource<T>();
            var cts = new CancellationTokenSource(timeoutMs);

            EventHandler<T> handler = null;
            handler = (sender, response) =>
            {
                if (predicate(response))
                {
                    tcs.TrySetResult(response);
                }
            };

            SubscribeHandler(handler);

            cts.Token.Register(() =>
            {
                tcs.TrySetResult(null);
                UnsubscribeHandler(handler);
            });

            tcs.Task.ContinueWith(_ => UnsubscribeHandler(handler), TaskScheduler.Default);

            return tcs.Task;
        }

        private void SubscribeHandler<T>(EventHandler<T> handler) where T : PacketBase
        {
            if (typeof(T) == typeof(AvailabilityResponse))
                AvailabilityResponseReceived += handler as EventHandler<AvailabilityResponse>;
            else if (typeof(T) == typeof(DetectionReport))
                DetectionReportReceived += handler as EventHandler<DetectionReport>;
            else if (typeof(T) == typeof(AdcDataPacket))
                AdcDataReceived += handler as EventHandler<AdcDataPacket>;
        }

        private void UnsubscribeHandler<T>(EventHandler<T> handler) where T : PacketBase
        {
            if (typeof(T) == typeof(AvailabilityResponse))
                AvailabilityResponseReceived -= handler as EventHandler<AvailabilityResponse>;
            else if (typeof(T) == typeof(DetectionReport))
                DetectionReportReceived -= handler as EventHandler<DetectionReport>;
            else if (typeof(T) == typeof(AdcDataPacket))
                AdcDataReceived -= handler as EventHandler<AdcDataPacket>;
        }

        public void Dispose()
        {
            StopListening();
            _udpClient?.Dispose();
        }
    }
}