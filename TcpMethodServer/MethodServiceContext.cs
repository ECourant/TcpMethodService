using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpMethodServer
{
    public sealed class MethodServiceContext
    {
        internal MethodServiceContext()
        {

        }
        public string IPAddress { get; private set; }
        public int Port { get; private set; }
        public MHeaderCollection Headers { get; private set; }
    }
}
