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
            _InnerProxy = new InnerProxy<T>(server, port, config);
            _IdProvider = Global.IdProvider;
            _contract = typeof(T).FullName;
        }

        protected override Task OnOpen()
        {
            return _InnerProxy.Open();
        }

        protected override Task OnClose()
        {
            return _InnerProxy.Close();
        }

        public Task SendOneWay(string method, params object[] msg)
        {
            var request = new Request(0, _contract, method, msg);
            return _InnerProxy.SendOneWay(request);
        }

        public Task SendVoid(string method, params object[] msg)
        {
            var request = CreateRequest(method, msg);
            return _InnerProxy.SendVoid(request);
        }

        public Task<R> SendReturn<R>(string method, params object[] msg)
        {
            var request = CreateRequest(method, msg);
            return _InnerProxy.SendReturn<R>(request);
        }

        Request CreateRequest(string method, params object[] msg)
        {
            var id = int.Parse(_IdProvider.NewId());
            return new Request(id, _contract, method, msg);
        }
    }
}
