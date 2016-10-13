using TcpServiceCore.Communication;
using TcpServiceCore.Dispatching;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace TcpServiceCore.Server
{
    public class ServiceHost<T> : CommunicationObject where T: new()
    {
        Type type;
        TcpListener listener;

        public event Action<T> ServiceInstantiated;

        IInstanceContextFactory<T> InstanceContextFactory = new InstanceContextFactory<T>();

        Dictionary<string, ChannelManager> ChannelManagers = new Dictionary<string, ChannelManager>();

        public ServiceHost(int port)
        {
            this.type = typeof(T);

            var endpoint = new IPEndPoint(IPAddress.Any, port);
            this.listener = new TcpListener(endpoint);
        }

        public void AddContract<TContract>(ChannelConfig config)
        {
            var contract = ContractDescription<TContract>.Create();

            contract.ValidateImplementation(this.type.GetTypeInfo());

            var cm = new ChannelManager(contract, config);

            this.ChannelManagers.Add(contract.ContractName, cm);
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
                        var socket = await this.listener.AcceptSocketAsync();
                        var handler = new ServerRequestHandler<T>(socket, this.ChannelManagers, this.InstanceContextFactory);
                        await handler.Open();
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
