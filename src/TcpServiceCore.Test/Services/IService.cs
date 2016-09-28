﻿using System.Threading.Tasks;
using TcpServiceCore.Attributes;

namespace TcpServiceCore.Test.Services
{
    [ServiceContract]
    public interface IService
    {

        [OperationContract]
        Task<string> Echo(string msg);

        [OperationContract(IsOneWay = true)]
        Task FireMsg(string msg);
    }
}
