using TcpServiceCore.Exceptions;
using TcpServiceCore.Serialization;
using TcpServiceCore.Serialization.Protobuf;
using TcpServiceCore.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpServiceCore
{
    public static class Global
    {
        public static ISerializer Serializer { get; set; }
        public static ExceptionHandler ExceptionHandler { get; set; }
        public static IMsgIdProvider IdProvider { get; set; }

        static Global()
        {
            Serializer = new ProtoSerializer();
            ExceptionHandler = new ExceptionHandler();
            IdProvider = new SimpleIdProvider();
        }
    }
}
