using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpServiceCore.Protocol
{
    abstract class ResponseStreamHandler : StreamHandler, IResponseHandler
    {
        public ResponseStreamHandler(TcpClient client)
                : base(client)
        {

        }

        public async Task<Response> GetResponse()
        {
            var data = await this.Read();

            var id = BitConverter.ToInt32(data, 0);

            var isError = BitConverter.ToBoolean(data, 4);

            var load = data.Skip(5).ToArray();

            var response = new Response(id, isError, load);

            return response;
        }

        public async Task WriteRequest(Request request)
        {
            var data = new List<byte>();

            data.AddRange(BitConverter.GetBytes(request.Id));

            var contractBytes = Encoding.ASCII.GetBytes(request.Contract);
            data.Add((byte)contractBytes.Length);
            data.AddRange(contractBytes);

            var operationBytes = Encoding.ASCII.GetBytes(request.Operation);
            data.Add((byte)operationBytes.Length);
            data.AddRange(operationBytes);

            data.AddRange(request.Parameter);

            await this.Write(data.ToArray());
        }

        protected abstract Task OnResponseReceived(Response response);

        protected override async Task OnRead()
        {
            var response = await this.GetResponse();
            await this.OnResponseReceived(response);
        }
    }
}
