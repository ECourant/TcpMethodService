using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpMethodServer
{
    internal enum ERequestType
    {
        SetBuffer,
        Connect,
        Disconnect,
        GetInfo,
        Invoke
    }
}
