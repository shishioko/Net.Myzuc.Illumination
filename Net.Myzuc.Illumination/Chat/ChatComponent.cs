using Newtonsoft.Json;
using System.Collections.Generic;

namespace Net.Myzuc.Illumination.Chat
{
    public abstract class ChatComponent
    {
        [JsonProperty("bold")]
        public bool? Bold { get; set; }
        [JsonProperty("italic")]
        public bool? Italic { get; set; }
        [JsonProperty("underlined")]
        public bool? Underlined { get; set; }
        [JsonProperty("strikethrough")]
        public bool? Strikethrough { get; set; }
        [JsonProperty("obfuscated")]
        public bool? Obfuscated { get; set; }
        [JsonProperty("font")]
        public string? Font { get; set; }
        [JsonProperty("color")]
        public string? Color { get; set; }
        [JsonProperty("insertion")]
        public string? Insertion { get; set; }
        [JsonProperty("clickEvent")]
        public ClickEvent? ClickEvent { get; set; }
        [JsonProperty("hoverEvent")]
        public HoverEvent? HoverEvent { get; set; }
        [JsonProperty("extra")]
        public IEnumerable<ChatComponent>? Extra { get; set; }
        public ChatComponent()
        {
            Bold = null;
            Italic = null;
            Underlined = null;
            Strikethrough = null;
            Obfuscated = null;
            Font = null;
            Color = null;
            Insertion = null;
            Extra = null;
        }
    }
}
