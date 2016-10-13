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
using TcpServiceCore.Dispatching;

namespace TcpServiceCore.Client
{
    public class InnerProxy : CommunicationObject, IClientChannel
    {
        readonly string server;
        readonly int port;
        readonly Socket socket;
        readonly AsyncStreamHandler streamHandler;
        readonly ChannelManager channelManager;
        readonly IMsgIdProvider idProvider;
        readonly string contract;
        readonly bool isNew;

        internal InnerProxy(Socket socket, ChannelManager channelManager)
        {
            this.idProvider = Global.IdProvider;

            this.socket = socket;
            if(this.socket == null)
                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            this.channelManager = channelManager;

            this.contract = this.channelManager.Contract.ContractName;

            this.socket.Configure(this.channelManager.Config);

            this.streamHandler = new AsyncStreamHandler(this.socket, this.channelManager.BufferManager);
        }

        public InnerProxy(string server, int port, ChannelManager channelManager)
            : this(null, channelManager)
        {
            this.server = server;
            this.port = port;
            isNew = true;
        }

        protected override async Task OnOpen()
        {
            if(this.isNew)
                await socket.ConnectAsync(server, port);
            await this.streamHandler.Open();
        }

        protected override async Task OnClose()
        {
            await this.streamHandler.Close();
            socket.Dispose();
        }

        public Task SendOneWay(string method, params object[] msg)
        {
            var request = new Message(MessageType.Request, 0, this.contract, method, msg);
            return this.streamHandler.WriteMessage(request);
        }

        public Task SendVoid(string method, params object[] msg)
        {
            var request = this.CreateRequest(method, msg);
            return this.streamHandler.WriteRequest(request, this.socket.ReceiveTimeout);
        }

        public async Task<R> SendReturn<R>(string method, params object[] msg)
        {
            var request = this.CreateRequest(method, msg);
            var response = await this.streamHandler.WriteRequest(request, this.socket.ReceiveTimeout);
            if (response.MessageType == MessageType.Error)
                throw new Exception(Global.Serializer.Deserialize<string>(response.Parameters[0]));
            var result = Global.Serializer.Deserialize<R>(response.Parameters[0]);
            return result;
        }

        Message CreateRequest(string method, params object[] msg)
        {
            var id = int.Parse(this.idProvider.NewId());
            return new Message(MessageType.Request, id, this.contract, method, msg);
        }
    }
}
