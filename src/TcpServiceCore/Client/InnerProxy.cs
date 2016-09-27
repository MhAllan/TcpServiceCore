using TcpServiceCore.Communication;
using TcpServiceCore.Protocol;
using System.Net.Sockets;
using System.Threading.Tasks;
using TcpServiceCore.Tools;

namespace TcpServiceCore.Client
{
    class InnerProxy<T> : CommunicationObject
    {
        readonly string server;
        readonly int port;
        TcpClient client;
        ResponseStreamHandler responseHandler;
        readonly ChannelConfig ChannelConfig;

        public InnerProxy(string server, int port, ChannelConfig channelConfig)
        {
            this.server = server;
            this.port = port;
            ChannelConfig = channelConfig;
            Init();
        }

        void Init()
        {
            client = new TcpClient(AddressFamily.InterNetwork);
            client.Configure(ChannelConfig);
            responseHandler = new ResponseStreamHandler(client);
        }

        protected override async Task OnOpen()
        {
            await client.ConnectAsync(server, port);
            await responseHandler.Open();
        }

        protected override async Task OnClose()
        {
            await responseHandler.Close();
            client.Dispose();
        }

        public async Task SendOneWay(Request request)
        {
            await responseHandler.WriteRequest(request);
        }

        public async Task SendVoid(Request request)
        {
            await responseHandler.WriteRequest(request, client.Client.ReceiveTimeout);
        }

        public async Task<R> SendReturn<R>(Request request)
        {
            Response response = await responseHandler.WriteRequest(request, client.Client.ReceiveTimeout);
            var result = Global.Serializer.Deserialize<R>(response.Value);
            return result;
        }
    }
}
