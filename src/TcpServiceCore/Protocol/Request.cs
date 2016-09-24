using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpServiceCore.Protocol
{
    class Request
    {
        public int Id { get; set; }

        public string Contract { get; set; }

        public string Operation { get; set; }

        public byte[] Parameter { get; set; }

        public Request(int id, string contract, string operation, object parameter)
        {
            this.Id = id;
            this.Contract = contract;
            this.Operation = operation;
            if (parameter is byte[])
                this.Parameter = (byte[])parameter;
            else
                this.Parameter = Global.Serializer.Serialize(parameter);
        }
    }
}
