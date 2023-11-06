using Newtonsoft.Json;

namespace Net.Myzuc.Illumination.Content.Chat
{
    public sealed class HoverEvent
    {
        [JsonProperty("show_text")]
        public ChatComponent? ShowText { get; set; }
        [JsonProperty("show_item")]
        public ItemPreview? ShowItem { get; set; }
        [JsonProperty("show_entity")]
        public string? ShowEntity { get; set; }
        public HoverEvent()
        {

        }
    }
}
