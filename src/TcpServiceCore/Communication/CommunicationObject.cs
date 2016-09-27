using System;
using System.Threading.Tasks;

namespace TcpServiceCore.Communication
{
    public abstract class CommunicationObject : ICommunicationObject
    {
        public CommunicationState State { get; private set; }

        protected CommunicationObject()
        {
            State = CommunicationState.Created;
        }

        protected abstract Task OnOpen();
        protected abstract Task OnClose();


        public async Task Open()
        {
            lock (this)
            {
                if (State != CommunicationState.Created)
                    throw new Exception($"Can not open channel when its state is {State.ToString()}");
                State = CommunicationState.Openning;
            }
            try
            {
                await OnOpen();
                lock (this)
                {
                    if (State != CommunicationState.Openning)
                        throw new Exception($"Can not open channel when its state is {State.ToString()}");
                    State = CommunicationState.Opened;
                }
            }
            catch (Exception)
            {
                lock (this)
                {
                    State = CommunicationState.Faulted;
                    throw;
                }
            }
        }

        public async Task Close()
        {
            lock (this)
            {
                if(State < CommunicationState.Opened)
                    throw new Exception($"Can not close channel when its state is {State.ToString()}");
                State = CommunicationState.Closing;
            }
            try
            {
                await OnClose();
                lock (this)
                {
                    if (State != CommunicationState.Closing)
                        throw new Exception($"Can not close channel when its state is {State.ToString()}");
                    State = CommunicationState.Closed;
                }
            }
            catch (Exception)
            {
                lock (this)
                {
                    State = CommunicationState.Faulted;
                    throw;
                }
            }
        }

        public virtual async Task Abort()
        {
            await OnClose();
        }

        public async void Dispose()
        {
            await Abort();
        }
    }
}
