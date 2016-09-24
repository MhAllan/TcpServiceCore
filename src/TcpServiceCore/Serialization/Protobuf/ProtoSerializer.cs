using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpServiceCore.Serialization.Protobuf
{
    public class ProtoSerializer : ISerializer
    {
        public object Deserialize(Type type, byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                return Serializer.Deserialize(type, ms);
            }
        }

        public T Deserialize<T>(byte[] data)
        {
            return (T)Deserialize(typeof(T), data);
        }

        public byte[] Serialize(object obj)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
