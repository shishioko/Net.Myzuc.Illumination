using Newtonsoft.Json;
using System.Collections.Generic;

namespace Me.Shishioko.Illumination.Chat
{
    public sealed class ChatTranslation : ChatComponent
    {
        [JsonProperty("translate")]
        public string Translate { get; set; }
        [JsonProperty("with")]
        public IEnumerable<string>? With { get; set; }
        public ChatTranslation(string translate, IEnumerable<string>? with = null)
        {
            Translate = translate;
            With = with;
        }
    }
}
