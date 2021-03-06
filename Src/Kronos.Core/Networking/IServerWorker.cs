﻿using System;
using Kronos.Core.Storage;

namespace Kronos.Core.Networking
{
    public interface IServerWorker : IDisposable
    {
        IStorage Storage { get; }

        void Start();
    }
}
