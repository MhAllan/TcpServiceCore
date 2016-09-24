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
        Task<int> Echo(string msg);

        [OperationContract(IsOneWay = true)]
        Task FireMsg(string msg);
    }
}
