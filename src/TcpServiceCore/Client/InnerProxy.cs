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
        ResponseStreamHandler responseHandler;
        ChannelConfig ChannelConfig;

        public InnerProxy(string server, int port, ChannelConfig channelConfig)
        {
            var type = typeof(T);
            this.server = server;
            this.port = port;
            this.ChannelConfig = channelConfig;
            this.Init();
        }

        void Init()
        {
            this.client = new TcpClient(AddressFamily.InterNetwork);
            this.client.Configure(ChannelConfig);
            this.responseHandler = new ResponseStreamHandler(this.client);
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

        public async Task SendOneWay(Request request)
        {
            await this.responseHandler.WriteRequest(request);
        }

        public async Task SendVoid(Request request)
        {
            await this.responseHandler.WriteRequest(request, this.client.Client.ReceiveTimeout);
        }

        public async Task<R> SendReturn<R>(Request request)
        {
            Response response = await this.responseHandler.WriteRequest(request, this.client.Client.ReceiveTimeout);
            var result = Global.Serializer.Deserialize<R>(response.Value);
            return result;
        }
    }
}
