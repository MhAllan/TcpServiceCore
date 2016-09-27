using System;
using System.Threading.Tasks;

namespace TcpServiceCore.Communication
{
    public interface ICommunicationObject : IDisposable
    {
        CommunicationState State { get; }
        Task Open();
        Task Close();
        Task Abort();
    }
}
