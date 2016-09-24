using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TcpServiceCore.Communication;

namespace TcpServiceCore.Protocol
{
    interface IResponseHandler : ICommunicationObject
    {
        Task<Response> GetResponse();
        Task WriteRequest(Request request);
    }
}
