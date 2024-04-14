using Newtonsoft.Json;

namespace Me.Shishioko.Illumination.Chat
{
    public sealed class ItemPreview
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("tag")]
        public string Tag { get; set; }
        public ItemPreview(string id, int count, string tag)
        {
            Id = id;
            Count = count;
            Tag = tag;
        }
    }
}
