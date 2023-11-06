using Net.Myzuc.Illumination.Content.Chat;
using Newtonsoft.Json;

namespace Net.Myzuc.Illumination.Content.Status
{
    public sealed class ServerStatus
    {
        [JsonProperty("version")]
        public ServerVersion? Version { get; set; }
        [JsonProperty("players")]
        public StatusPlayerInfo? Players { get; set; }
        [JsonProperty("description")]
        public ChatComponent? Description { get; set; }
        [JsonProperty("favicon")]
        public string? Favicon { get; set; }
        [JsonProperty("enforcesSecureChat")]
        public bool? EnforcesSecureChat { get; set; }
        public ServerStatus()
        {
            Version = null;
            Players = null;
            Description = null;
            Favicon = null;
            EnforcesSecureChat = null;
        }
    }
}
