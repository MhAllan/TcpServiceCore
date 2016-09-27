using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpServiceCore.Protocol
{
    class ResponseStreamHandler : StreamHandler, IResponseHandler
    {
        readonly ConcurrentDictionary<int, ResponseEvent> mapper = new ConcurrentDictionary<int, ResponseEvent>();

        public ResponseStreamHandler(TcpClient client)
                : base(client)
        {
        }

        async Task<Response> GetResponse()
        {
            var data = await Read();

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

            await Write(data.ToArray());
        }

        public async Task<Response> WriteRequest(Request request, int timeout)
        {
            var responseEvent = new ResponseEvent();
            if (!mapper.TryAdd(request.Id, responseEvent))
            {
                Dispose();
                throw new Exception("Could not add request to the mapper");
            }
            await WriteRequest(request);
            return responseEvent.GetResponse(timeout);
        }

        protected override async Task OnRead()
        {
            var response = await GetResponse();
            ResponseEvent responseEvent;
            mapper.TryRemove(response.Id, out responseEvent);
            responseEvent.SetResponse(response);
            await Task.CompletedTask;
        }

        private class ResponseEvent
        {
            Response _response;
            private bool IsSuccess { get; set; }
            private bool IsCompleted { get; set; }

            readonly ManualResetEvent Evt;

            public ResponseEvent()
            {
                Evt = new ManualResetEvent(false);
            }

            public void SetResponse(Response response)
            {
                IsSuccess = true;
                _response = response;
                Evt.Set();
            }

            public Response GetResponse(int timeout)
            {
                Evt.WaitOne(timeout);
                IsCompleted = true;

                if (IsSuccess == false)
                    throw new Exception("Receivetimeout reached without getting response");
                return _response;
            }
        }
    }
}
