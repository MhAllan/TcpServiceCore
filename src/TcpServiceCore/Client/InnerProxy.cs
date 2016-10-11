using TcpServiceCore.Communication;
using TcpServiceCore.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TcpServiceCore.Tools;

namespace TcpServiceCore.Client
{
    class InnerProxy<T> : CommunicationObject
    {
        string server;
        int port;
        TcpClient client;
        AsyncStreamHandler responseHandler;
        ChannelManager ChannelManager;

        public InnerProxy(string server, int port, ChannelManager channelManager)
        {
            this.server = server;
            this.port = port;
            this.ChannelManager = channelManager;
            this.Init();
        }

        void Init()
        {
            this.client = new TcpClient(AddressFamily.InterNetwork);
            this.client.Configure(ChannelManager.ChannelConfig);

            this.responseHandler = new AsyncStreamHandler(this.client, this.ChannelManager.BufferManager);
        }

        protected override async Task OnOpen()
        {
            await client.ConnectAsync(server, port);
            await this.responseHandler.Open();
        }

        protected override async Task OnClose()
        {
            await this.responseHandler.Close();
            client.Dispose();
        }

        public async Task SendOneWay(Message request)
        {
            await this.responseHandler.WriteMessage(request);
        }

        public async Task SendVoid(Message request)
        {
            await this.responseHandler.WriteRequest(request, this.client.Client.ReceiveTimeout);
        }

        public async Task<R> SendReturn<R>(Message request)
        {
            var response = await this.responseHandler.WriteRequest(request, this.client.Client.ReceiveTimeout);
            if (response.MessageType == MessageType.Error)
                throw new Exception(Global.Serializer.Deserialize<string>(response.Parameters[0]));
            var result = Global.Serializer.Deserialize<R>(response.Parameters[0]);
            return result;
        }
    }
}
