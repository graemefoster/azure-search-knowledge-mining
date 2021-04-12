using Newtonsoft.Json;

namespace Azure.KnowledgeMining
{
    public class Emotion
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}