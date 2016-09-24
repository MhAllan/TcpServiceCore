using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using TcpServiceCore.Protocol;

namespace TcpServiceCore.Client
{
    //callback is not implemented yet
    class ClientCallbackHandler<T> : RequestStreamHandler
    {
        public ClientCallbackHandler(TcpClient client)
            : base(client)
        {

        }
        protected override Task OnRequestReceived(Request request)
        {
            throw new NotImplementedException();
        }
    }
}
