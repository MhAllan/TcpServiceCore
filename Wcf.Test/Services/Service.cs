using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Wcf.Test.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Service : IService
    {
        public string Echo(string msg)
        {
            return msg.ToUpper();
        }

        public Task<string> EchoAsync(string msg)
        {
            return Task.FromResult(msg.ToUpper());
        }

        public void FireMsg(string msg)
        {
            Console.WriteLine($"received {msg}");
        }
    }
}
