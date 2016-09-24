using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TcpServiceCore.Communication
{
    public abstract class CommunicationObject : ICommunicationObject
    {
        public CommunicationState State { get; private set; }

        public CommunicationObject()
        {
            this.State = CommunicationState.Created;
        }

        protected abstract Task OnOpen();
        protected abstract Task OnClose();


        public async Task Open()
        {
            lock (this)
            {
                if (this.State != CommunicationState.Created)
                    throw new Exception($"Can not open channel when its state is {State.ToString()}");
                this.State = CommunicationState.Openning;
            }
            try
            {
                await this.OnOpen();
                lock (this)
                {
                    if (this.State != CommunicationState.Openning)
                        throw new Exception($"Can not open channel when its state is {State.ToString()}");
                    this.State = CommunicationState.Opened;
                }
            }
            catch (Exception)
            {
                lock (this)
                {
                    this.State = CommunicationState.Faulted;
                    throw;
                }
            }
        }

        public async Task Close()
        {
            lock (this)
            {
                if(this.State < CommunicationState.Opened)
                    throw new Exception($"Can not close channel when its state is {State.ToString()}");
                this.State = CommunicationState.Closing;
            }
            try
            {
                await this.OnClose();
                lock (this)
                {
                    if (this.State != CommunicationState.Closing)
                        throw new Exception($"Can not close channel when its state is {State.ToString()}");
                    this.State = CommunicationState.Closed;
                }
            }
            catch (Exception)
            {
                lock (this)
                {
                    this.State = CommunicationState.Faulted;
                    throw;
                }
            }
        }

        public virtual async Task Abort()
        {
            await this.OnClose();
        }

        public async void Dispose()
        {
            await this.Abort();
        }
    }
}
