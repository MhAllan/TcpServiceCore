using TcpServiceCore.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TcpServiceCore.Buffering;
using System.Threading;
using System.Collections.Concurrent;

namespace TcpServiceCore.Protocol
{
    abstract class StreamHandler : CommunicationObject
    {
        ConcurrentDictionary<int, ResponseEvent> mapper = new ConcurrentDictionary<int, ResponseEvent>();

        protected readonly TcpClient Client;

        protected IBufferManager BufferManager { get; set; }

        public StreamHandler(TcpClient client, IBufferManager bufferManager)
        {
            this.Client = client;
            this.BufferManager = bufferManager;
        }

        public virtual bool CanRead
        {
            get { return this.Client.Client.Connected; }
        }

        protected abstract Task _Write(byte[] data, int offset, int length);
        protected abstract Task<int> _Read(byte[] buffer, int offset, int length);

        protected virtual Task _OnRequestReceived(Message request) { return Task.CompletedTask; }

        protected override Task OnOpen()
        {
            Task.Run(async () =>
            {
                while (this.State == CommunicationState.Openning)
                {
                    //waiting the open state.
                }
                while (this.State == CommunicationState.Opened)
                {
                    var msg = await this.ReadMessage();
                    if (msg.MessageType == MessageType.Response || msg.MessageType == MessageType.Error)
                    {
                        ResponseEvent responseEvent;
                        this.mapper.TryRemove(msg.Id, out responseEvent);
                        responseEvent.SetResponse(msg);
                    }
                    else if (msg.MessageType == MessageType.Request)
                    {
                        await this._OnRequestReceived(msg);
                    }
                }
            });
            return Task.CompletedTask;
        }

        protected override Task OnClose()
        {
            return Task.CompletedTask;
        }

        public async Task<Message> ReadMessage()
        {
            this.ThrowIfNotOpened();

            var size = await this.GetMessageSize();

            var buffer = this.BufferManager.GetFitBuffer(size);
            
            var index = 0;

            var data = await this.ReadBytes(buffer, size);

            var msgType = (MessageType)data[index];

            index += 1;

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

            var load = data.Skip(index).Take(size - index).ToArray();

            var request = new Message(msgType, id, contract, method, load);

            this.BufferManager.AddBuffer(data);

            return request;
        }

        public async Task WriteMessage(Message request)
        {
            this.ThrowIfNotOpened();

            var data = new List<byte>();

            data.Add((byte)request.MessageType);

            data.AddRange(BitConverter.GetBytes(request.Id));

            var contractBytes = Encoding.ASCII.GetBytes(request.Contract);
            data.Add((byte)contractBytes.Length);
            data.AddRange(contractBytes);

            var operationBytes = Encoding.ASCII.GetBytes(request.Operation);
            data.Add((byte)operationBytes.Length);
            data.AddRange(operationBytes);

            data.AddRange(request.Parameter);

            var dataSize = BitConverter.GetBytes(data.Count);

            var msg = new List<byte>();
            msg.AddRange(dataSize);
            msg.AddRange(data);

            await this._Write(msg.ToArray(), 0, msg.Count);
        }

        async Task<int> GetMessageSize()
        {
            var size = 4;
            var buffer = this.BufferManager.GetFitBuffer(size);
            await this.ReadBytes(buffer, 4);
            var result = BitConverter.ToInt32(buffer, 0);
            this.BufferManager.AddBuffer(buffer);
            return result;
        }

        async Task<byte[]> ReadBytes(byte[] result, int length)
        {
            var read = 0;
            while (this.State == CommunicationState.Opened && this.CanRead)
            {
                read += await this._Read(result, read, length - read);
                if (read == length)
                {
                    return result;
                }
            }
            throw new Exception("Stream is not readable");
        }

        public async Task<Message> WriteRequest(Message request, int timeout)
        {
            var responseEvent = new ResponseEvent();
            if (!this.mapper.TryAdd(request.Id, responseEvent))
            {
                this.Dispose();
                throw new Exception("Could not add request to the mapper");
            }
            await this.WriteMessage(request);
            return responseEvent.GetResponse(timeout);
        }

        private class ResponseEvent
        {
            Message _response;
            public bool IsSuccess { get; set; }
            public bool IsCompleted { get; private set; }

            ManualResetEvent Evt;

            public ResponseEvent()
            {
                this.Evt = new ManualResetEvent(false);
            }

            public void SetResponse(Message response)
            {
                this.IsSuccess = true;
                this._response = response;
                this.Evt.Set();
            }

            public Message GetResponse(int timeout)
            {
                this.Evt.WaitOne(timeout);
                this.IsCompleted = true;

                if (this.IsSuccess == false)
                    throw new Exception("Receivetimeout reached without getting response");
                return _response;
            }
        }
    }
}
