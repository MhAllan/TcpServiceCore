using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TcpServiceCore.Client;
using TcpServiceCore.Communication;
using TcpServiceCore.Server;
using TcpServiceCore.Test.Services;

namespace TcpServiceCore.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            RunExample().Wait();
        }

        static async Task RunExample()
        {
            var config = new ChannelConfig
            {
                ReceiveTimeout = TimeSpan.FromSeconds(10),
                SendTimeout = TimeSpan.FromSeconds(10)
            };

            var host = new ServiceHost<Service>(9091);

            host.AddContract<IService>(config);

            host.ServiceInstanciated += s =>
            {
                //construct the created instance
            };

            await host.Open();

            var client = await ChannelFactory<IService>.CreateProxy("localhost", 9091, config, true);

            using ((IClientChannel)client)
            {
                var result = await client.Echo("message");
                Console.WriteLine(result);
            }

            //client = await ChannelFactory<IService>.CreateProxy("localhost", 9091, config);
            //var channel = (IClientChannel)client;

            //await channel.Open();


            //await channel.Close();

            Console.ReadLine();
        }
    }
}
