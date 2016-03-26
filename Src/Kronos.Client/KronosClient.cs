﻿using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Kronos.Client.Transfer;
using Kronos.Core.Communication;
using Kronos.Core.Configuration;
using Kronos.Core.Requests;
using Kronos.Core.StatusCodes;

namespace Kronos.Client
{
    /// <summary>
    /// Official Kronos client
    /// <see cref="IKronosClient" />
    /// </summary>
    internal class KronosClient : IKronosClient
    {
        private readonly ServerProvider _serverProvider;
        private readonly Func<IPEndPoint, IClientServerConnection> _connectionResolver;

        public KronosClient(KronosConfig config) : this(config, endpoint => new SocketCommunicationService(endpoint))
        {
        }

        internal KronosClient(KronosConfig config, Func<IPEndPoint, IClientServerConnection> connectionResolver)
        {
            _serverProvider = new ServerProvider(config.ClusterConfig);
            _connectionResolver = connectionResolver;
        }

        public void Insert(string key, byte[] package, DateTime expiryDate)
        {
            Trace.WriteLine("New insert request");
            InsertRequest request = new InsertRequest(key, package, expiryDate);

            IClientServerConnection connection = SelectServerAndCreateConnection(key);
            RequestStatusCode status = request.Execute<RequestStatusCode>(connection);

            Trace.WriteLine($"InsertRequest status: {status}");
        }

        public Task InsertAsync(string key, byte[] package, DateTime expiryDate)
        {
            return Task.Run(() => Insert(key, package, expiryDate));
        }

        public byte[] Get(string key)
        {
            Trace.WriteLine("New get request");
            GetRequest request = new GetRequest(key);

            IClientServerConnection connection = SelectServerAndCreateConnection(key);
            byte[] valueFromCache = request.Execute<byte[]>(connection);

            if (valueFromCache != null && valueFromCache.Length == 1 && valueFromCache[0] == 0)
                return null;

            return valueFromCache;
        }

        public Task<byte[]> GetAsync(string key)
        {
            return Task.Run(() => Get(key));
        }

        public void Delete(string key)
        {
            Trace.WriteLine("New delete request");
            DeleteRequest request = new DeleteRequest(key);

            IClientServerConnection connection = SelectServerAndCreateConnection(key);
            RequestStatusCode status = request.Execute<RequestStatusCode>(connection);

            Trace.WriteLine($"InsertRequest status: {status}");
        }

        public Task DeleteAsync(string key)
        {
            return Task.Run(() => Delete(key));
        }

        private IClientServerConnection SelectServerAndCreateConnection(string key)
        {
            ServerConfig server = _serverProvider.SelectServer(key.GetHashCode());
            Trace.WriteLine($"Selected server {server}");
            IClientServerConnection connection = _connectionResolver(server.EndPoint);
            return connection;
        }
    }
}
