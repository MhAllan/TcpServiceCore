using TcpServiceCore.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace TcpServiceCore.Dispatching
{
    class InstanceContext<T> where T: new()
    {
        static ThreadLocal<InstanceContext<T>> _Current = new ThreadLocal<InstanceContext<T>>();

        public static InstanceContext<T> Current
        {
            get
            {
                return _Current.Value;
            }
            set
            {
                _Current.Value = value;
            }
        }

        public readonly T Service;
        readonly TypeDispatcher<T> Dispatcher;

        public InstanceContext(TypeDispatcher<T> dispatcher)
        {
            this.Dispatcher = dispatcher;
            this.Service = this.Dispatcher.CreateInstance();
        }

        public async Task<Message> HandleRequest(Message request)
        {
            Message response = null;

            var operation = this.Dispatcher.GetOperation(request.Operation);

            var result = await operation.Execute(this.Service, request);

            if (operation.IsOneWay == false)
            {
                if (operation.IsVoidTask)
                {
                    response = new Message(MessageType.Response, request.Id, (byte)1);
                }
                else
                {
                    response = new Message(MessageType.Response, request.Id, result);
                }
            }
            return response;
        }
    }
}
