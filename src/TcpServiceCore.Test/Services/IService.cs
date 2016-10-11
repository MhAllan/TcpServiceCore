using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TcpServiceCore.Attributes;

namespace TcpServiceCore.Test.Services
{
    [ServiceContract]
    public interface IService
    {

        [OperationContract]
        Task<string> Echo(string msg);

        [OperationContract]
        Task<Msg> EchoMsg(Msg msg);

        [OperationContract]
        Task<string> EchoNoParam();

        [OperationContract]
        Task<string> EchoServerError();

        [OperationContract(IsOneWay = true)]
        Task FireMsg(string msg);
    }
}
