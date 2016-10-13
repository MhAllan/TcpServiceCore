﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpServiceCore.Dispatching
{
    class InstanceContextFactory<T> : IInstanceContextFactory<T> where T: new()
    {
        public event Action<T> ServiceInstantiated;
        //Create instace context, no static so we can have two hosts in one application
        ConcurrentDictionary<Socket, InstanceContext<T>> contexts =
            new ConcurrentDictionary<Socket, InstanceContext<T>>();

        object _lock = new object();

        InstanceContext<T> Singleton;

        InstanceContext<T> IInstanceContextFactory<T>.Create(Socket socket)
        {
            InstanceContext<T> result = null;
            if (InstanceContext<T>.InstanceContextMode == InstanceContextMode.Single)
            {
                if (Singleton == null)
                {
                    lock (_lock)
                    {
                        if (Singleton == null)
                            Singleton = new InstanceContext<T>();
                    }
                }
                result = Singleton;
            }
            else if (InstanceContext<T>.InstanceContextMode == InstanceContextMode.PerCall)
            {
                result = new InstanceContext<T>();
            }
            else if (InstanceContext<T>.InstanceContextMode == InstanceContextMode.PerSession)
            {
                result = contexts.AddOrUpdate(socket, new InstanceContext<T>(), (s, d) => d);
            }
            return result;
        }
    }
}
