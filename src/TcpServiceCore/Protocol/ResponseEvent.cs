using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TcpServiceCore.Protocol
{
    class ResponseEvent
    {
        Response _response;
        public bool IsSuccess { get; set; }
        public bool IsCompleted { get; private set; }

        ManualResetEvent Evt;

        public ResponseEvent()
        {
            this.Evt = new ManualResetEvent(false);
        }

        public void SetResponse(Response response)
        {
            this.IsSuccess = true;
            this._response = response;
            this.Evt.Set();
        }

        public Response GetResponse(int timeout)
        {
            this.Evt.WaitOne(timeout);
            this.IsCompleted = true;

            if (this.IsSuccess == false)
                throw new Exception("Receivetimeout reached without getting response");
            return _response;
        }
    }
}
