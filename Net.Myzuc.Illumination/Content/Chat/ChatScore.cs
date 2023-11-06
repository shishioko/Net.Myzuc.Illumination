using Newtonsoft.Json;

namespace Net.Myzuc.Illumination.Content.Chat
{
    public sealed class ChatScore : ChatComponent
    {

        [JsonProperty("score")]
        public ResolvedScore Score { get; set; }
        public ChatScore(ResolvedScore score)
        {
            Score = score;
        }
    }
}
