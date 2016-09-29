using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TcpServiceCore.Buffering
{
    public class BufferManagerFactory
    {
        public virtual IBufferManager CreateBufferManager(int maxBufferSize, int maxBufferPoolSize)
        {
            return new BufferManager(maxBufferSize, maxBufferPoolSize);
        }
    }
}
