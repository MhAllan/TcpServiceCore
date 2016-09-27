using System;
using System.Collections.Concurrent;

namespace TcpServiceCore.Tools
{
    class SimpleIdProvider : IMsgIdProvider
    {
        readonly ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
        string id;
        public SimpleIdProvider(string id = null)
        {
            this.id = string.IsNullOrEmpty(id) ? Environment.TickCount.ToString() : id;
            for (int i = 0; i < 1000; i++)
            {
                queue.Enqueue(i.ToString());
            }
        }
        public string NewId()
        {
            string next;
            if (queue.TryDequeue(out next) == false)
            {
                next = NewId();
            }
            else
            {
                queue.Enqueue(next);
            }
            return next;
        }
    }
}
