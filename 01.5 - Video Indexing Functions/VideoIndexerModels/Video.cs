using Newtonsoft.Json;

namespace Azure.KnowledgeMining
{
    public class Video
    {
        [JsonProperty("insights")]
        public Insight Insights { get; set; }
    }
}