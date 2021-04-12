using Newtonsoft.Json;

namespace Azure.KnowledgeMining
{
    public class VideoIndexerResult
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("summarizedInsights")]
        public SummarizedInsights SummarizedInsights { get; set; }
        
        [JsonProperty("videos")]
        public Video[] Videos { get; set; }
    }
}