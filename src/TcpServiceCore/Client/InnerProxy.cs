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
    public class InnerProxy<T> : CommunicationObject, IClientChannel
    {
        static readonly string _contract;

        static InnerProxy()
        {
            _contract = typeof(T).FullName;
        }

        string server;
        int port;
        TcpClient client;
        AsyncStreamHandler responseHandler;
        ChannelManager ChannelManager;

        IMsgIdProvider _IdProvider;

        public InnerProxy(string server, int port, ChannelConfig config)
        {
            this._IdProvider = Global.IdProvider;
            this.server = server;
            this.port = port;
            this.ChannelManager = new ChannelManager(_contract, config);
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

        public Task SendOneWay(string method, params object[] msg)
        {
            var request = new Message(MessageType.Request, 0, _contract, method, msg);
            return this.responseHandler.WriteMessage(request);
        }

        public Task SendVoid(string method, params object[] msg)
        {
            var request = this.CreateRequest(method, msg);
            return this.responseHandler.WriteRequest(request, this.client.Client.ReceiveTimeout);
        }

        public async Task<R> SendReturn<R>(string method, params object[] msg)
        {
            var request = this.CreateRequest(method, msg);
            var response = await this.responseHandler.WriteRequest(request, this.client.Client.ReceiveTimeout);
            if (response.MessageType == MessageType.Error)
                throw new Exception(Global.Serializer.Deserialize<string>(response.Parameters[0]));
            var result = Global.Serializer.Deserialize<R>(response.Parameters[0]);
            return result;
        }

        Message CreateRequest(string method, params object[] msg)
        {
            var id = int.Parse(this._IdProvider.NewId());
            return new Message(MessageType.Request, id, _contract, method, msg);
        }
    }
}
