using PAAOM_Common.Network.Interfaces;
using PAAOM_Common.Network.Models;
using System;
using System.ComponentModel;
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

        public event EventHandler<AvailabilityRequest> AvailabilityRequestReceived;
        public event EventHandler<AvailabilityResponse> AvailabilityResponseReceived;
        public event EventHandler<DetectionReport> DetectionReportReceived;
        public event EventHandler<AdcDataPacket> AdcDataReceived;
        public event PropertyChangedEventHandler PropertyChanged;

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
                StopListening();

                _udpClient = new UdpClient();
                _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpClient.Client.Bind(localEndpoint);
                _remoteEndPoint = remoteEndpoint;

                StartListening();
            }
            catch (SocketException ex)
            {
                throw new Exception($"Не удалось занять порт {localEndpoint.Port}: {ex.Message}", ex);
            }
        }

        public void StartListening()
        {
            if (_isListening) return;

            try
            {
                _isListening = true;
                _ = Task.Run(ListenLoop);
            }
            catch (Exception ex)
            {
                _isListening = false;
                throw new Exception("Failed to start listening", ex);
            }
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
            if (!_isListening)
                throw new InvalidOperationException("Service is not listening");

            var tcs = new TaskCompletionSource<bool>();
            var cts = new CancellationTokenSource(timeoutMs);

            // Обработчик для конкретного пакета
            void Handler(object sender, AvailabilityResponse response)
            {
                if (response.PacketId == packetId)
                {
                    tcs.TrySetResult(true);
                }
            }

            AvailabilityResponseReceived += Handler;

            // Отмена по таймеру
            cts.Token.Register(() =>
            {
                tcs.TrySetResult(false);
                AvailabilityResponseReceived -= Handler;
            });

            try
            {
                // Отправка запроса
                var request = new AvailabilityRequest { PacketId = packetId };
                await SendAsync(request);

                return await tcs.Task;
            }
            finally
            {
                AvailabilityResponseReceived -= Handler;
                cts.Dispose();
            }
        }

        private async Task ListenLoop()
        {
            while (_isListening && _udpClient != null)
            {
                try
                {
                    UdpReceiveResult result = await _udpClient.ReceiveAsync();
                    if (_packetBuilder.TryParsePacket(result.Buffer, out var packet))
                    {
                        HandleReceivedPacket(packet, result.RemoteEndPoint);
                    }
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // Логируем ошибку, но продолжаем слушать
                    Console.WriteLine($"Listen error: {ex.Message}");
                    await Task.Delay(1000);
                }
            }
        }

        private void HandleReceivedPacket(PacketBase packet, IPEndPoint sender)
        {
            switch (packet)
            {
                case AvailabilityRequest request: 
                    AvailabilityRequestReceived?.Invoke(this, request);
                    break;
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

        public void Dispose()
        {
            StopListening();
            _udpClient?.Dispose();
        }
    }
}