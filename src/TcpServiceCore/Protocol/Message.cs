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

        public Message(MessageType msgType, int id, object parameter)
        {
            this.MessageType = msgType;
            this.Id = id;
            if (parameter is byte[])
                this.Parameter = (byte[])parameter;
            else
                this.Parameter = Global.Serializer.Serialize(parameter);

            this.Contract = string.Empty;
            this.Operation = string.Empty;
        }

        public Message(MessageType msgType, int id, string contract, string operation, object parameter)
            : this(msgType, id, parameter)
        {
            this.Contract = contract;
            this.Operation = operation;
        }
    }
}
