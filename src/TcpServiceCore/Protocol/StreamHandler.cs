using TcpServiceCore.Communication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TcpServiceCore.Protocol
{
    abstract class StreamHandler : CommunicationObject
    {
        public event Action<TcpClient> Disconnected;

        protected TcpClient Client;
        protected NetworkStream Stream;

        protected StreamHandler(TcpClient client)
        {
            Client = client;
        }

        protected abstract Task OnRead();

        protected override Task OnOpen()
        {
            Stream = Client.GetStream();
            Task.Run(async () =>
            {
                try
                {
                    while (Stream.CanRead)
                    {
                        await OnRead();
                    }
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.Message);
                    //Global.ExceptionHandler?.LogException(ex);
                }
                finally
                {
                    Dispose();
                }
            });
            return Task.CompletedTask;
        }

        protected async Task<byte[]> Read()
        {
            var size = await GetMessageSize();
            var msg = await ReadBytes(size);
            return msg;
        }

        protected async Task Write(byte[] data)
        {
            var msg = new List<byte>();
            var dataSize = BitConverter.GetBytes(data.Length);

            msg.AddRange(dataSize);
            msg.AddRange(data);

            await Stream.WriteAsync(msg.ToArray(), 0, msg.Count);
        }

        async Task<int> GetMessageSize()
        {
            var bytes = await ReadBytes(4);
            return BitConverter.ToInt32(bytes, 0);
        }

        async Task<byte[]> ReadBytes(int length)
        {
            var result = new byte[length];
            var read = 0;
            while (State == CommunicationState.Opened && Stream.CanRead)
            {
                read += await Stream.ReadAsync(result, read, length - read);
                if (read == length)
                {
                    return result;
                }
            }
            await Close();
            throw new Exception("Stream is not readable");
        }

        protected override Task OnClose()
        {
            Client.Dispose();
            Disconnected?.Invoke(Client);
            return Task.CompletedTask;
        }
    }
}
