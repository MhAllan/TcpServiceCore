using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TcpServiceCore.Buffering;

namespace TcpServiceCore.Communication
{
    class ChannelManager
    {
        public readonly string Contract;
        public readonly ChannelConfig ChannelConfig;
        public readonly IBufferManager BufferManager;

        public ChannelManager(string contract, ChannelConfig config)
        {
            this.Contract = contract;

            this.ChannelConfig = config;

            var mbs = config.MaxBufferSize;
            var mps = config.MaxBufferPoolSize;

            this.BufferManager = Global.BufferManagerFactory.CreateBufferManager(contract, mbs, mps);
        }

    }
}
