using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using TcpServiceCore.Communication;
using TcpServiceCore.Protocol;
using TcpServiceCore.Tools;

namespace TcpServiceCore.Client
{
    //callback is not implemented yet
    class ClientHandler :  ResponseStreamHandler
    {
        ConcurrentDictionary<int, Action<Response>> mapper = new ConcurrentDictionary<int, Action<Response>>();

        public ClientHandler(TcpClient client)
            : base(client)
        {

        }

        protected override Task OnResponseReceived(Response response)
        {
            Action<Response> action;
            this.mapper.TryRemove(response.Id, out action);
            action.Invoke(response);
            return Task.CompletedTask;
        }

        public void RegisterRequest(Request request, Action<Response> handler)
        {
            if (!this.mapper.TryAdd(request.Id, handler))
            {
                this.Dispose();
                throw new Exception("Could not add request to the mapper");
            }
        }
    }
}
