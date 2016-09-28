using System;
using System.Threading.Tasks;
using TcpServiceCore.Attributes;
using TcpServiceCore.Dispatching;

namespace TcpServiceCore.Test.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Service : IService
    {
        public Task<string> Echo(string msg)
        {
            return Task.FromResult(msg.ToUpper());
        }

        public Task FireMsg(string msg)
        {
            Console.WriteLine($"received {msg}");
            return Task.CompletedTask;
        }
    }
}
