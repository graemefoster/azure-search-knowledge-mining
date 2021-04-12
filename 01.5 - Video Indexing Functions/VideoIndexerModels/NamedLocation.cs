using Newtonsoft.Json;

namespace Azure.KnowledgeMining
{
    public class NamedLocation
    {
        [JsonProperty("confidence")]
        public double Confidence { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}