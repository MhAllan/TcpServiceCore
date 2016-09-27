using System;
using System.Collections.Generic;

namespace TcpServiceCore.Buffering
{
    public class BufferPool
    {
        readonly Queue<byte[]> buffers = new Queue<byte[]>();

        public readonly int BufferSize;
        public readonly int PoolSize;
        public readonly int MaxBuffersCount;

        public BufferPool(int bufferSize, int poolSize)
        {
            const string bsn = nameof(bufferSize);
            const string psn = nameof(poolSize);
            if (bufferSize <= 0)
                throw new Exception($"{bsn} must be positive greater than 0");
            if (poolSize <= 0)
                throw new Exception($"{psn} must be positive greater than 0");
            if (poolSize < bufferSize)
                throw new Exception($"{psn} can not be less than {bsn}");

            BufferSize = bufferSize;
            PoolSize = poolSize;
            MaxBuffersCount = (int)Math.Floor((double)PoolSize / BufferSize);
        }

        public byte[] GetBuffer()
        {
            byte[] buffer;
            lock (buffers)
            {
                if (buffers.Count == 0)
                {
                    buffer = new byte[BufferSize];
                    buffers.Enqueue(buffer);
                }
                else
                {
                    buffer = buffers.Dequeue();
                }
            }
            return buffer;
        }

        public void AddBuffer(byte[] buffer)
        {
            if (buffer == null || buffer.Length != BufferSize)
                return;

            if (buffers.Count < MaxBuffersCount)
            {
                lock (buffers)
                {
                    if (buffers.Count < MaxBuffersCount)
                    {
                        buffers.Enqueue(buffer);
                    }
                }
            }
        }

    }
}
