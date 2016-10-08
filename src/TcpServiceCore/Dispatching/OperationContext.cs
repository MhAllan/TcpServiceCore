using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TcpServiceCore.Protocol;

namespace TcpServiceCore.Dispatching
{
    public sealed class OperationContext
    {
        static ThreadLocal<OperationContext> _Current = new ThreadLocal<OperationContext>();

        public static OperationContext Current
        {
            get { return _Current.Value; }
        }

        //TODO, hide the TcpClient, Expose proxy instead
        public readonly TcpClient Client;

        readonly object Service;
        readonly MethodOperation Operation;

        internal OperationContext(object service, TcpClient client, MethodOperation operation)
        {
            this.Service = service;
            this.Client = client;
            this.Operation = operation;
            _Current.Value = this;
        }

        internal async Task<Message> HandleRequest(Message request)
        {
            Message response = null;

            if (this.Operation.IsOneWay)
            {
                await this.Operation.Execute(this.Service, request);
            }
            else
            {
                try
                {
                    var result = await this.Operation.Execute(this.Service, request);
                    if (this.Operation.IsVoidTask)
                    {
                        response = new Message(MessageType.Response, request.Id, (byte)1);
                    }
                    else
                    {
                        response = new Message(MessageType.Response, request.Id, result);
                    }
                }
                catch
                {
                    response = new Message(MessageType.Error, request.Id, "Server Error");
                }
            }

            return response;
        }
    }
}
