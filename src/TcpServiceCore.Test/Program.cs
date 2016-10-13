using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                ReceiveTimeout = TimeSpan.FromSeconds(20),
                SendTimeout = TimeSpan.FromSeconds(20)
            };

            var host = new ServiceHost<Service>(9091);

            host.AddContract<IService>(config);

            host.ServiceInstantiated += s =>
            {
                //construct the created instance
            };

            await host.Open();
          
            var client = await ChannelFactory<IService>.CreateProxy("localhost", 9091, config, true);

            using ((IClientChannel)client)
            {
                var str = await client.Echo("message");
                Console.WriteLine(str);

                var msg = new Msg { Id = 1, Body = "From Client" };
                var result = await client.EchoMsg(msg);
                Console.WriteLine(result.Body);

                var msg1 = new Msg { Body = "M1" };
                var msg2 = new Msg { Body = "M2" };

                var multiParams = await client.EchoMany(msg1, msg2, "hello world", -123);
                Console.WriteLine(multiParams.Body);

                var noParam = await client.EchoNoParam();
                Console.WriteLine(noParam);

                //try
                //{
                //    var err = await client.EchoServerError();
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine(ex.Message);
                //}
            }

            //client = await ChannelFactory<IService>.CreateProxy("localhost", 9091, config);
            //var channel = (IClientChannel)client;

            //await channel.Open();


            //await channel.Close();

            Console.ReadLine();
        }
    }
}
