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
          var bytes = value as byte[];
          this.Value = bytes ?? Global.Serializer.Serialize(value);
        }

        public Response()
        {

        }

    }
}
