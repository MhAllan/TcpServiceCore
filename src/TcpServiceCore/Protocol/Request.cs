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
            Id = id;
            Contract = contract;
            Operation = operation;
            Parameter = parameter as byte[] ?? Global.Serializer.Serialize(parameter);
        }
    }
}
