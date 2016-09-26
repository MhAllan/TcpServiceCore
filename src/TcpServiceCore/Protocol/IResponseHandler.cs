using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcpServiceCore.Communication;

namespace TcpServiceCore.Protocol
{
    interface IResponseHandler : ICommunicationObject
    {
        Task WriteRequest(Request request);
        Task<Response> WriteRequest(Request request, int timeout);
    }
}
