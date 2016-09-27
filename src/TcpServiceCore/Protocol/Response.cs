namespace TcpServiceCore.Protocol
{
    class Response
    {
        public int Id { get; set; }

        public bool IsError { get; set; }
        
        public byte[] Value{ get; set; }

        public Response(int id, bool isError, object value)
        {
            Id = id;
            IsError = isError;
            var bytes = value as byte[];
            Value = bytes ?? Global.Serializer.Serialize(value);
        }

        public Response()
        {

        }

    }
}
