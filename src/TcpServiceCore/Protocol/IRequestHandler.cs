using System.Threading.Tasks;
using TcpServiceCore.Communication;

namespace TcpServiceCore.Protocol
{
    interface IRequestHandler : ICommunicationObject
    {
        Task<Request> GetRequest();
        Task WriteResponse(Response response);
    }
}
