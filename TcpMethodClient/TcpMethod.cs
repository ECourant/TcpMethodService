using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpMethodClient
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class TcpMethod : Attribute
    {
        public string MethodName { get; set; } = string.Empty;
        public bool FireAndForget { get; set; } = false;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(Defaults.TimeoutMinutes);
    }
}
