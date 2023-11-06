using Newtonsoft.Json;

namespace Net.Myzuc.Illumination.Content.Chat
{
    public sealed class ChatText : ChatComponent
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        public ChatText(string text)
        {
            Text = text;
        }
    }
}
