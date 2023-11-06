using Newtonsoft.Json;

namespace Net.Myzuc.Illumination.Content.Chat
{
    public sealed class ClickEvent
    {
        [JsonProperty("open_url")]
        public string? OpenUrl { get; set; }
        [JsonProperty("run_command")]
        public string? RunCommand { get; set; }
        [JsonProperty("suggest_command")]
        public string? SuggestCommand { get; set; }
        [JsonProperty("change_page")]
        public int? ChangePage { get; set; }
        [JsonProperty("copy_to_clipboard")]
        public string? CopyToClipboard { get; set; }
        public ClickEvent()
        {

        }
    }
}
