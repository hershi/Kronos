﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Kronos.Core.Networking;
using Kronos.Core.Processing;
using Kronos.Server.EventArgs;
using NLog;

namespace Kronos.Server.Listening
{
    public class Listener : IListener
    {
        public int ActiveConnections => _activeConnections;

        private int _activeConnections;

        private readonly TcpListener _listener;
        private readonly IProcessor _processor;
        private readonly IRequestProcessor _requestProcessor;
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public Listener(IPAddress ipAddress, int port, IProcessor processor, IRequestProcessor requestProcessor)
        {
            _listener = new TcpListener(ipAddress, port);
            _processor = processor;
            _requestProcessor = requestProcessor;
        }

        public void Start()
        {
            _logger.Info("Starting Kronos Server");
            _listener.Start();
            _logger.Info($"Kronos has been started on {_listener.LocalEndpoint}");

            CancellationToken token = _cancel.Token;

            Task.Factory.StartNew(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        Socket socket = await _listener.AcceptSocketAsync().ConfigureAwait(false);
                        ProcessSocketConnection(socket);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Exception during accepting new request {ex}");
                    }
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public void Stop()
        {
            try
            {
                _cancel.Cancel();
                _listener.Stop();
                _logger.Info("Server has been stopped");
            }
            catch (SocketException ex)
            {
                _logger.Error($"Exception during disposing server: {ex}");
            }
        }

        public void Dispose()
        {
            _logger.Info("Stopping TCP/IP server");
            Stop();
        }

        private async Task ProcessSocketConnection(Socket socket)
        {
            Interlocked.Increment(ref _activeConnections);
            string id = Guid.NewGuid().ToString();
            RequestArgs request = await _processor.ReceiveRequestAsync(socket).ConfigureAwait(false);
            try
            {
                _logger.Debug($"Processing new request {request.Type} with Id: {id}, {request.Received} bytes");
                byte[] response = _requestProcessor.Handle(request.Type, request.Request, request.Received);
                _logger.Debug($"Sending response with {response.Length} bytes to the user");
                await SocketUtils.SendAllAsync(socket, response).ConfigureAwait(false);

                _logger.Debug($"Processing {id} finished");
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception on processing request {id}, {ex}");
            }

            Interlocked.Decrement(ref _activeConnections);
        }
    }
}
