using System;

namespace TcpServiceCore.Communication
{
    public class ChannelConfig
    {
        //public int MaxBufferSize { get; set; }
        //public int MaxPoolBufferSize { get; set; }
        public TimeSpan ReceiveTimeout { get; set; }
        public TimeSpan SendTimeout { get; set; }
    }
}
