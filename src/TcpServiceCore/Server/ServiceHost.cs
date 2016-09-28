using TcpServiceCore.Communication;
using TcpServiceCore.Dispatching;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Reflection;

namespace TcpServiceCore.Server
{
    public class ServiceHost<T> : CommunicationObject where T: new()
    {
        readonly Type type;
        readonly TcpListener listener;

        public event Action<T> ServiceInstantiated;

        readonly IInstanceContextFactory<T> InstanceContextFactory = new InstanceContextFactory<T>();

        readonly Dictionary<string, ChannelConfig> ChannelConfigs = new Dictionary<string, ChannelConfig>();

        public ServiceHost(int port)
        {
            this.type = typeof(T);

            var endpoint = new IPEndPoint(IPAddress.Any, port);
            this.listener = new TcpListener(endpoint);
        }

        public void AddContract<Contract>(ChannelConfig config)
        {
            var cType = typeof(Contract);
            ContractHelper.ValidateContract(this.type.GetTypeInfo(), cType.GetTypeInfo());
            this.ChannelConfigs.Add(cType.FullName, config);
        }

        protected override Task OnOpen()
        {
            this.InstanceContextFactory.ServiceInstantiated += this.ServiceInstantiated;
            this.listener.Start(3000);
            Task.Run(async () =>
            {
                while (this.State == CommunicationState.Opened)
                {
                    try
                    {
                        var client = await this.listener.AcceptTcpClientAsync();
                        var handler = new ServerRequestHandler<T>(client, this.ChannelConfigs, this.InstanceContextFactory);
                        handler.Open();
                    }
                    catch (Exception ex)
                    {
                        Global.ExceptionHandler?.LogException(ex);
                    }
                }
            });
            return Task.CompletedTask;
        }

        protected override Task OnClose()
        {
            this.listener.Stop();
            return Task.CompletedTask;
        }
    }
}
