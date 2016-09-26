using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpServiceCore.Protocol
{
    abstract class RequestStreamHandler : StreamHandler, IRequestHandler
    {
        public RequestStreamHandler(TcpClient client)
            : base(client)
        {

        }


        public async Task<Request> GetRequest()
        {
            var index = 0;
            var data = await this.Read();

            var id = BitConverter.ToInt32(data, index);

            index += 4;

            var contractLength = data[index];

            index += 1;

            var contract = Encoding.ASCII.GetString(data, index, contractLength);

            index += contractLength;

            var methodLength = data[index];

            index += 1;

            var method = Encoding.ASCII.GetString(data, index, methodLength);

            index += methodLength;

            var load = data.Skip(index).ToArray();

            var request = new Request(id, contract, method, load);

            return request;
        }

        public async Task WriteResponse(Response response)
        {
            var data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(response.Id));
            data.AddRange(BitConverter.GetBytes(response.IsError));
            data.AddRange(response.Value);

            await this.Write(data.ToArray());
        }

        protected abstract Task OnRequestReceived(Request request);

        protected override async Task OnRead()
        {
            var request = await this.GetRequest();
            await this.OnRequestReceived(request);
        }
    }
}
