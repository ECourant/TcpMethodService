using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpMethodClient
{
    public sealed class MHeader
    {
        public MHeader(string Name, object Value)
        {
            this.Name = Name;
            this.Value = Value;
        }
        public string Name { get; private set; }
        public object Value { get; set; }
    }
}
