using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpMethodClient
{
    internal class RequestQueue<T> : System.Collections.Concurrent.ConcurrentQueue<T> where T : MRequest
    {
        public new void Enqueue(T item)
        {
            this.Enqueued.Invoke(item, EventArgs.Empty);
            base.Enqueue(item);
        }

        public EventHandler Enqueued;
    }
}
