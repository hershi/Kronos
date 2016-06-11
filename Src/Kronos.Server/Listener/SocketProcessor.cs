﻿using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Kronos.Core.Requests;
using Kronos.Core.Serialization;
using Kronos.Core.StatusCodes;
using NLog;
using XGain;
using XGain.Processing;
using XGain.Sockets;

namespace Kronos.Server.Listener
{
    public class SocketProcessor : IProcessor<MessageArgs>
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public async Task<MessageArgs> ProcessSocketConnectionAsync(ISocket client)
        {
            _logger.Debug("Accepting new request");
            MessageArgs args = null;
            try
            {
                byte[] typeBuffer = await ReceiveAndSendConfirmation(client);
                byte[] requestBuffer = await ReceiveAndSendConfirmation(client);

                RequestType type = SerializationUtils.Deserialize<RequestType>(typeBuffer);

                args = new MessageArgs(client, requestBuffer, type);
            }
            catch (SocketException ex)
            {
                _logger.Error(
                    $"Exception during receiving request from client {client?.RemoteEndPoint} + {ex}");
            }

            return args;
        }

        private async Task<byte[]> ReceiveAndSendConfirmation(ISocket socket)
        {
            byte[] packageSizeBuffer = new byte[sizeof(int)];
            _logger.Debug("Receiving information about request size");
            int position = socket.Receive(packageSizeBuffer);

            int requestSize = SerializationUtils.GetLengthOfPackage(packageSizeBuffer);
            _logger.Debug($"Request contains {requestSize} bytes");

            using (MemoryStream ms = new MemoryStream())
            {
                await ms.WriteAsync(packageSizeBuffer, 0, position);
                position = 0;
                while (position != requestSize)
                {
                    byte[] package = new byte[socket.BufferSize];

                    int received = socket.Receive(package);
                    _logger.Debug($"Received {received} bytes");

                    await ms.WriteAsync(package, 0, received);
                    position += received;
                    _logger.Debug($"Total received bytes: {(float)position * 100 / requestSize}%");
                }

                // send confirmation
                byte[] statusBuffer = SerializationUtils.Serialize(RequestStatusCode.Ok);
                socket.Send(statusBuffer);

                return ms.ToArray();
            }
        }
    }
}
