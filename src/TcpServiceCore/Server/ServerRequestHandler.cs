using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TcpServiceCore.Buffering;
using TcpServiceCore.Communication;
using TcpServiceCore.Dispatching;
using TcpServiceCore.Protocol;
using TcpServiceCore.Tools;

namespace TcpServiceCore.Server
{
    class ServerRequestHandler<T> : AsyncStreamHandler where T: new()
    {
        IInstanceContextFactory<T> instanceContextFactory;

        Dictionary<string, ChannelConfig> channelConfigs;
        bool isAccepted;

        public ServerRequestHandler(TcpClient client, 
            Dictionary<string, ChannelConfig> channelConfigs,
            IInstanceContextFactory<T> instanceContextFactory)
            : base(client, new DummyBufferManager())
        {
            this.instanceContextFactory = instanceContextFactory;
            this.channelConfigs = channelConfigs;
        }

        protected override async Task _OnRequestReceived(Message request)
        {
            if (isAccepted)
            {
                await DoHandleRequest(request);
            }
            else
            {
                var contract = request.Contract;
                if (string.IsNullOrEmpty(contract))
                    throw new Exception($"Wrong socket initialization, Request.Contract should not be null or empty");
                var channelConfig = this.channelConfigs.FirstOrDefault(x => x.Key == contract);

                var config = channelConfig.Value;
                if (config == null)
                    throw new Exception($"Wrong socket initialization, contract {contract} is missing");

                this.Client.Configure(config);

                var mbs = config.MaxBufferSize;
                var mps = config.MaxBufferPoolSize;
                this.BufferManager = Global.BufferManagerFactory.CreateBufferManager(mbs, mps);

                isAccepted = true;

                await DoHandleRequest(request);
            }
        }

        async Task DoHandleRequest(Message request)
        {
            var context = this.instanceContextFactory.Create(this.Client);
            var response = await context.HandleRequest(request);
            if (response != null)
                await this.WriteMessage(response);
        }
    }
}
