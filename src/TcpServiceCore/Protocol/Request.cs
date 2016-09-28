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
          var bytes = parameter as byte[];
          this.Parameter = bytes ?? Global.Serializer.Serialize(parameter);
        }
    }
}
