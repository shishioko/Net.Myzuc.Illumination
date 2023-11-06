using Newtonsoft.Json;
using System;

namespace Net.Myzuc.Illumination.Content.Status
{
    public sealed class StatusPlayer
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("id")]
        public Guid Id { get; set; }
        public StatusPlayer(string name, Guid id)
        {
            Name = name;
            Id = id;
        }
    }
}
