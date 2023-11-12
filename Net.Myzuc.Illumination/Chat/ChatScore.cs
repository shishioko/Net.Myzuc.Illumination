using Newtonsoft.Json;

namespace Net.Myzuc.Illumination.Chat
{
    public sealed class ChatScore : ChatComponent
    {
        public sealed class ResolvedScore
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("objective")]
            public string Objective { get; set; }
            [JsonProperty("value")]
            public int Value { get; set; }
            public ResolvedScore(string name, string objective, int value)
            {
                Name = name;
                Objective = objective;
                Value = value;
            }
        }

        [JsonProperty("score")]
        public ResolvedScore Score { get; set; }
        public ChatScore(ResolvedScore score)
        {
            Score = score;
        }
    }
}
