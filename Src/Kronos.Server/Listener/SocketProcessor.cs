﻿using System;
using System.Buffers;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using Kronos.Core.Communication;
using Kronos.Core.Requests;
using Kronos.Core.Serialization;
using NLog;
using XGain;
using XGain.Processing;
using XGain.Sockets;

namespace Kronos.Server.Listener
{
    public class SocketProcessor : IProcessor<MessageArgs>
    {
        public ArrayPool<byte> BytesPool { get; }

        private const int IntSize = sizeof(int);
        private const int RequestTypeSize = sizeof(ushort);

        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public SocketProcessor()
        {
            BytesPool = ArrayPool<byte>.Create();
        }

        public Task<MessageArgs> ProcessSocketConnectionAsync(ISocket client)
        {
            MessageArgs args = null;
            try
            {
                ReceivedMessage msg = ReceiveMessageAsync(client);
                args = new MessageArgs(client, msg.Data, msg.Type);
            }
            catch (SocketException ex)
            {
                _logger.Error(
                    $"Exception during receiving request from client {client?.RemoteEndPoint} + {ex}");
            }

            return Task.FromResult(args);
        }

        private ReceivedMessage ReceiveMessageAsync(ISocket socket)
        {
            byte[] lengthBuffer = new byte[IntSize]; // TODO stackalloc
            SocketUtils.ReceiveAll(socket, lengthBuffer, IntSize);
            int dataLength = BitConverter.ToInt32(lengthBuffer, 0);
            Debug.Assert(dataLength != 0);

            byte[] typeBuffer = new byte[RequestTypeSize]; // todo stackalloc;
            SocketUtils.ReceiveAll(socket, typeBuffer, RequestTypeSize);
            RequestType requestType = SerializationUtils.Deserialize<RequestType>(typeBuffer, RequestTypeSize);
            Debug.Assert(requestType != RequestType.Unknown);

            byte[] data = new byte[dataLength - RequestTypeSize]; // todo array pooling
            SocketUtils.ReceiveAll(socket, data, data.Length);

            return new ReceivedMessage(requestType, data);
        }
    }
}
