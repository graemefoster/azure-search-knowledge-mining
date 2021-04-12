using Newtonsoft.Json;

namespace Azure.KnowledgeMining
{
    public class Insight
    {
        [JsonProperty("transcript")]
        public Transcript[] Transcript { get; set; }
    }
}