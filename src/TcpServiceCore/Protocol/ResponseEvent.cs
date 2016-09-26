using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TcpServiceCore.Protocol
{
    class ResponseEvent
    {
        public Response Response { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsCompleted { get; private set; }

        ManualResetEvent Evt;
        public ResponseEvent()
        {
            this.Response = null;
            this.IsSuccess = false;
            this.Evt = new ManualResetEvent(false);
        }

        public void Wait(int timeout)
        {
            this.Evt.WaitOne(timeout);
            this.IsCompleted = true;
        }

        public void Notify(Response response)
        {
            this.IsSuccess = true;
            this.Response = response;
            this.Evt.Set();
        }
    }
}
