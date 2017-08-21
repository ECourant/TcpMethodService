using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace TcpMethodServer
{
    internal sealed class MResponse
    {
        [JsonProperty("RequestID")]
        internal Guid RequestID { get; set; }
        [JsonProperty("ClientID")]
        internal Guid ClientID { get; set; }
        [JsonProperty("ServerID")]
        internal Guid ServerID { get; set; }
        [JsonProperty("Response")]
        internal object Response { get; set; }
        [JsonProperty("Headers")]
        internal Dictionary<string, string> Headers { get; set; }
        [JsonProperty("Exception")]
        internal Exception Exception { get; set; }
    }
}
