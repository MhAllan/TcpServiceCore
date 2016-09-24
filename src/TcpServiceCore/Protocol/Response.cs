using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TcpServiceCore.Protocol
{
    class Response
    {
        public int Id { get; set; }

        public bool IsError { get; set; }
        
        public byte[] Value{ get; set; }

        public Response(int id, bool isError, object value)
        {
            this.Id = id;
            this.IsError = IsError;
            if (value is byte[])
                this.Value = (byte[])value;
            else
                this.Value = Global.Serializer.Serialize(value);
        }

        public Response()
        {

        }

    }
}
