using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace TcpMethodServer
{

    internal class MRequest
    {
        [JsonIgnore]
        internal string Address { get; set; }
        [JsonProperty("RequestID")]
        internal Guid RequestID { get; set; }
        [JsonProperty("CliendID")]
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
