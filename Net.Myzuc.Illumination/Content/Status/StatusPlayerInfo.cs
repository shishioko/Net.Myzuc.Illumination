using Newtonsoft.Json;
using System.Collections.Generic;

namespace Net.Myzuc.Illumination.Content.Status
{
    public sealed class StatusPlayerInfo
    {
        [JsonProperty("max")]
        public int? Max { get; set; }
        [JsonProperty("online")]
        public int? Online { get; set; }
        [JsonProperty("sample")]
        public IEnumerable<StatusPlayer>? Sample { get; set; }
        public StatusPlayerInfo()
        {
            Max = null;
            Online = null;
            Sample = null;
        }
    }
}
