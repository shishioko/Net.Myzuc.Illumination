using Newtonsoft.Json;

namespace Net.Myzuc.Illumination.Content.Chat
{
    internal sealed class ChatKeybind : ChatComponent
    {
        [JsonProperty("keybind")]
        public string Keybind { get; set; }
        public ChatKeybind(string keybind)
        {
            Keybind = keybind;
        }
    }
}
