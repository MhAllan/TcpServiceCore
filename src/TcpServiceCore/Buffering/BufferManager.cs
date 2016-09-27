using System;
using System.Collections.Generic;

namespace TcpServiceCore.Buffering
{
    public class BufferManager : IBufferManager
    {
        public readonly int MaxBufferSize;
        public readonly int MaxBufferPoolSize;

        private const int minSize = 128;
        readonly Dictionary<int, BufferPool> pools = new Dictionary<int, BufferPool>();
        
        public BufferManager(int maxBufferSize, int maxBufferPoolSize)
        {
            const string bsn = nameof(maxBufferSize);
            const string psn = nameof(maxBufferPoolSize);

            if (maxBufferSize <= minSize)
                throw new Exception($"{bsn} must be positive greater than {minSize}");
            if (maxBufferPoolSize <= minSize)
                throw new Exception($"{psn} must be positive greater than {minSize}");
            if (maxBufferPoolSize < maxBufferSize)
                throw new Exception($"{psn} can not be less than {bsn}");

            MaxBufferSize = maxBufferSize;
            MaxBufferPoolSize = maxBufferPoolSize;

            var poolSize = minSize / 2;
            do
            {
                poolSize = Math.Min(poolSize * 2, maxBufferPoolSize);
                pools.Add(poolSize, new BufferPool(poolSize, maxBufferPoolSize));
            } while (poolSize < maxBufferPoolSize);
        }

        public byte[] GetFitBuffer(int size)
        {
            if (size > MaxBufferSize)
                throw new Exception($"Received message is too big, max buffer size is {MaxBufferSize}");

            if (size < minSize)
                return new byte[size];

            var fitPoolIndex = (int)Math.Ceiling((double)size / minSize);

            return pools[fitPoolIndex].GetBuffer();
        }

        public void AddBuffer(byte[] buffer)
        {
            if (buffer == null)
                return;

            var length = buffer.Length;

            if (length < minSize || length > MaxBufferSize)
                return;

            var fitPoolIndex = (int)Math.Ceiling((double)length / minSize);

            pools[fitPoolIndex].AddBuffer(buffer);
        }
    }
}
