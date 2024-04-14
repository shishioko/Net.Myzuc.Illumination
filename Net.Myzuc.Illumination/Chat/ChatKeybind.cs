using Newtonsoft.Json;

namespace Me.Shishioko.Illumination.Chat
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
