﻿using Kronos.Core.Networking;
using Kronos.Core.Requests;
using Kronos.Core.Serialization;
using Kronos.Core.Storage;
using XGain.Sockets;

namespace Kronos.Core.Processing
{
    public class GetProcessor : CommandProcessor<GetRequest, byte[]>
    {
        public override RequestType Type { get; } = RequestType.Get;

        public override void Handle(ref GetRequest request, IStorage storage, ISocket client)
        {
            byte[] response;
            if (!storage.TryGet(request.Key, out response))
            {
                response = SerializationUtils.Serialize(RequestStatusCode.NotFound);
            }

            Reply(response, client);
        }
    }
}