using Newtonsoft.Json;

namespace Azure.KnowledgeMining
{
    public class Sentiment
    {
        [JsonProperty("sentimentKey")]
        public string SentimentKey { get; set; }
    }
}