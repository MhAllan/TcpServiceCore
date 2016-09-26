using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpServiceCore.Protocol
{
    class ResponseStreamHandler : StreamHandler, IResponseHandler
    {
        ConcurrentDictionary<int, ResponseEvent> mapper = new ConcurrentDictionary<int, ResponseEvent>();

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

        public async Task<ResponseEvent> WriteRequest(Request request, bool isOneWay)
        {
            ResponseEvent result = null;
            if (!isOneWay)
            {
                result = new ResponseEvent();
                if (!this.mapper.TryAdd(request.Id, result))
                {
                    this.Dispose();
                    throw new Exception("Could not add request to the mapper");
                }
            }

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

            return result;
        }

        protected override async Task OnRead()
        {
            var response = await this.GetResponse();
            ResponseEvent responseEvent;
            this.mapper.TryRemove(response.Id, out responseEvent);
            responseEvent.SetResponse(response);
            await Task.CompletedTask;
        }
    }
}
