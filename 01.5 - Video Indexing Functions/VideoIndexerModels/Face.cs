using Newtonsoft.Json;

namespace Azure.KnowledgeMining
{
    public class Face
    {
        [JsonProperty("confidence")]
        public double Confidence { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("title")]
        public string Title { get; set; }
    }
}