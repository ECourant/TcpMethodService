using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpMethodServer
{
    public abstract class MethodService
    {
        internal MethodService()
        {
            throw new InvalidOperationException("MethodService objects cannot be initialized without a method context!");
        }
        internal MethodService(MethodServiceContext Context)
        {

        }
        protected MethodServiceContext Context { get; private set; }


    }
}
