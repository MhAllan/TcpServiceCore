using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TcpServiceCore.Buffering;

namespace TcpServiceCore.Communication
{
    class ChannelManager
    {
        public readonly ChannelConfig ChannelConfig;
        public readonly IBufferManager BufferManager;

        public ChannelManager(ChannelConfig config)
        {
            this.ChannelConfig = config;

            var mbs = config.MaxBufferSize;
            var mps = config.MaxBufferPoolSize;

            this.BufferManager = Global.BufferManagerFactory.CreateBufferManager(mbs, mps);
        }

    }
}
