using TcpServiceCore.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TcpServiceCore.Buffering;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading;

namespace TcpServiceCore.Protocol
{
    class AsyncStreamHandler : StreamHandler
    {
        protected NetworkStream Stream { get; private set; }

        public override bool CanRead
        {
            get
            {
                return this.Stream.CanRead;
            }
        }

        public AsyncStreamHandler(TcpClient client, IBufferManager bufferManager)
            : base(client, bufferManager)
        {
            
        }

        protected override async Task OnOpen()
        {
            await base.OnOpen();
            this.Stream = this.Client.GetStream();
        }

        protected override async Task<int> _Read(byte[] buffer, int offset, int length)
        {
            return await this.Stream.ReadAsync(buffer, offset, length);
        }

        protected override async Task _Write(byte[] buffer, int offset, int length)
        {
            await this.Stream.WriteAsync(buffer.ToArray(), offset, length);
        }
    }
}
