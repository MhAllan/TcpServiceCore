using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpServiceCore.Protocol
{
    class Message
    {
        public int Id { get; set; }

        public MessageType MessageType { get; set; }

        public string Contract { get; set; }

        public string Operation { get; set; }

        public byte[] Parameter { get; set; }

        public Message(MessageType msgType, int id, params object[] parameter)
        {
            this.MessageType = msgType;
            this.Id = id;
            if (parameter?.Length > 0)
            {
                var p = parameter[0];
                if (p is byte[])
                    this.Parameter = (byte[])p;
                else
                    this.Parameter = Global.Serializer.Serialize(p);
            }
            else
            {
                this.Parameter = new byte[0];
            }
            this.Contract = string.Empty;
            this.Operation = string.Empty;
        }

        public Message(MessageType msgType, int id, string contract, string operation, params object[] parameter)
            : this(msgType, id, parameter)
        {
            this.Contract = contract;
            this.Operation = operation;
        }
    }
}
