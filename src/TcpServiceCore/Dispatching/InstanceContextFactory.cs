using TcpServiceCore.Communication;
using TcpServiceCore.Protocol;
using System;
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
        public event Action<T> ServiceInstanciated;
        //Create instace context, no static so we can have two hosts in one application
        ConcurrentDictionary<TcpClient, InstanceContext<T>> contexts =
            new ConcurrentDictionary<TcpClient, InstanceContext<T>>();

        object _lock = new object();

        InstanceContext<T> Singleton;

        InstanceContext<T> IInstanceContextFactory<T>.Create(TcpClient client)
        {
            InstanceContext<T> result = null;
            var dispatcher = TypeDispatcher<T>.Instance;
            if (dispatcher.InstanceContextMode == InstanceContextMode.Single)
            {
                if (Singleton == null)
                {
                    lock (_lock)
                    {
                        if (Singleton == null)
                            Singleton = new InstanceContext<T>(dispatcher);
                    }
                }
                result = Singleton;
            }
            else if (dispatcher.InstanceContextMode == InstanceContextMode.PerCall)
            {
                result = new InstanceContext<T>(dispatcher);
            }
            else if (dispatcher.InstanceContextMode == InstanceContextMode.PerSession)
            {
                result = contexts.AddOrUpdate(client, new InstanceContext<T>(dispatcher), (s, d) => d);
            }
            if (InstanceContext<T>.Current != result)
            {
                this.ServiceInstanciated?.Invoke(result.Service);
            }
            InstanceContext<T>.Current = result;
            return result;
        }
    }
}
