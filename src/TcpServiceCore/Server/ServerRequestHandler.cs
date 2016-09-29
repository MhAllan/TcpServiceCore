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

        Dictionary<string, ChannelManager> channelManagers;
        bool isAccepted;

        public ServerRequestHandler(TcpClient client, 
            Dictionary<string, ChannelManager> channelManagers,
            IInstanceContextFactory<T> instanceContextFactory)
            : base(client, new DummyBufferManager())
        {
            this.instanceContextFactory = instanceContextFactory;
            this.channelManagers = channelManagers;
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

                ChannelManager cm = null;
                try
                {
                    cm = this.channelManagers[contract];
                    var config = cm.ChannelConfig;

                    this.Client.Configure(config);

                    var mbs = config.MaxBufferSize;
                    var mps = config.MaxBufferPoolSize;
                    this.BufferManager = cm.BufferManager;

                    isAccepted = true;

                    await DoHandleRequest(request);
                }
                catch
                {
                    if (cm == null)
                    {
                        var error = $"Wrong socket initialization, contract {contract} is missing";
                        try
                        {
                            var response = new Message(MessageType.Error, request.Id, error);
                            await this.WriteMessage(response);
                        }
                        catch
                        {

                        }
                        throw new Exception(error);
                    }
                    throw;
                }
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
