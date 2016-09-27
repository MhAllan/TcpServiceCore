using System;
using System.Net.Sockets;

namespace TcpServiceCore.Dispatching
{
    interface IInstanceContextFactory<T> where T: new()
    {
        event Action<T> ServiceInstantiated;
        InstanceContext<T> Create(TcpClient client);
    }
}
