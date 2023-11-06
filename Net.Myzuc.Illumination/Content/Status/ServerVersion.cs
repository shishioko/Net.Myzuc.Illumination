using Newtonsoft.Json;

namespace Net.Myzuc.Illumination.Content.Status
{
    public sealed class ServerVersion
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("protocol")]
        public int Protocol { get; set; }
        public ServerVersion(string name, int protocol)
        {
            Name = name;
            Protocol = protocol;
        }
    }
}
