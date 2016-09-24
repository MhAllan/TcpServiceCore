using TcpServiceCore.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpServiceCore.Protocol
{
    abstract class StreamHandler : CommunicationObject
    {
        public event Action<TcpClient> Disconnected;

        protected TcpClient Client;
        protected NetworkStream Stream;

        public StreamHandler(TcpClient client)
        {
            this.Client = client;
        }

        protected abstract Task OnRead();

        protected override Task OnOpen()
        {
            this.Stream = this.Client.GetStream();
            Task.Run(async () =>
            {
                try
                {
                    while (this.Stream.CanRead)
                    {
                        await this.OnRead();
                    }
                }
                catch (Exception ex)
                {
                    //Global.ExceptionHandler?.LogException(ex);
                }
                finally
                {
                    this.Dispose();
                }
            });
            return Task.CompletedTask;
        }

        protected async Task<byte[]> Read()
        {
            var size = await this.GetMessageSize();
            var msg = await this.ReadBytes(size);
            return msg;
        }

        protected async Task Write(byte[] data)
        {
            var msg = new List<byte>();
            var dataSize = BitConverter.GetBytes(data.Length);

            msg.AddRange(dataSize);
            msg.AddRange(data);

            await this.Stream.WriteAsync(msg.ToArray(), 0, msg.Count);
        }

        async Task<int> GetMessageSize()
        {
            var bytes = await this.ReadBytes(4);
            return BitConverter.ToInt32(bytes, 0);
        }

        async Task<byte[]> ReadBytes(int length)
        {
            var result = new byte[length];
            var read = 0;
            while (this.State == CommunicationState.Opened && this.Stream.CanRead)
            {
                read += await this.Stream.ReadAsync(result, read, length - read);
                if (read == length)
                {
                    return result;
                }
            }
            await this.Close();
            throw new Exception("Stream is not readable");
        }

        protected override Task OnClose()
        {
            this.Client.Dispose();
            this.Disconnected?.Invoke(this.Client);
            return Task.CompletedTask;
        }
    }
}
