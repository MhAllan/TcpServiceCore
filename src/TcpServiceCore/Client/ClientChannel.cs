using TcpServiceCore.Communication;
using TcpServiceCore.Protocol;
using TcpServiceCore.Tools;
using System.Threading.Tasks;

namespace TcpServiceCore.Client
{
    public class ClientChannel<T> : CommunicationObject, IClientChannel
    {
        readonly InnerProxy<T> _InnerProxy;
        readonly IMsgIdProvider _IdProvider;
        readonly string _contract;
        public ClientChannel(string server, int port, ChannelConfig config)
        {
            this._InnerProxy = new InnerProxy<T>(server, port, config);
            this._IdProvider = Global.IdProvider;
            this._contract = typeof(T).FullName;
        }

        protected override Task OnOpen()
        {
            return this._InnerProxy.Open();
        }

        protected override Task OnClose()
        {
            return this._InnerProxy.Close();
        }

        public Task SendOneWay(string method, params object[] msg)
        {
            var request = new Request(0, _contract, method, msg);
            return this._InnerProxy.SendOneWay(request);
        }

        public Task SendVoid(string method, params object[] msg)
        {
            var request = this.CreateRequest(method, msg);
            return this._InnerProxy.SendVoid(request);
        }

        public Task<R> SendReturn<R>(string method, params object[] msg)
        {
            var request = this.CreateRequest(method, msg);
            return this._InnerProxy.SendReturn<R>(request);
        }

        Request CreateRequest(string method, params object[] msg)
        {
            var id = int.Parse(this._IdProvider.NewId());
            return new Request(id, _contract, method, msg);
        }
    }
}
