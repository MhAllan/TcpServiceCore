using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Wcf.Test.Services
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        string Echo(string msg);

        [OperationContract]
        Task<string> EchoAsync(string msg);

        [OperationContract(IsOneWay = true)]
        void FireMsg(string msg);
    }
}
