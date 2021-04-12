using Newtonsoft.Json;

namespace Azure.KnowledgeMining
{
    public class Topic
    {
        [JsonProperty("confidence")]
        public double Confidence { get; set; }
        
        [JsonProperty("iabName")]
        public string IabName { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}