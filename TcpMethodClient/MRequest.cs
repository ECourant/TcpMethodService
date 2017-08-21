using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace TcpMethodClient
{
    internal class MRequest
    {
        [JsonProperty("RequestID")]
        internal Guid RequestID { get; set; }
        [JsonProperty("ClientID")]
        internal Guid ClientID { get; set; }
        [JsonProperty("RequestType"), JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        internal ERequestType RequestType { get; set; }
        [JsonProperty("Service")]
        internal string Service { get; set; }
        [JsonProperty("Method")]
        internal string Method { get; set; }
        [JsonProperty("Arguments")]
        internal object[] Arguments { get; set; }
        [JsonProperty("Headers")]
        internal Dictionary<string, string> Headers { get; set; }
    }
}
