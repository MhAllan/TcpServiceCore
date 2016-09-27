﻿using System.Net.Sockets;
using TcpServiceCore.Communication;

namespace TcpServiceCore.Tools
{
    public static class SocketExtensions
    {
        public static void Configure(this TcpClient client, ChannelConfig channelConfig)
        {
            client.Client.Configure(channelConfig);
        }
        public static void Configure(this Socket socket, ChannelConfig channelConfig)
        {
            socket.ReceiveTimeout = (int)channelConfig.ReceiveTimeout.TotalMilliseconds;
            socket.SendTimeout = (int)channelConfig.SendTimeout.TotalMilliseconds;
            socket.NoDelay = true;
        }
    }
}
