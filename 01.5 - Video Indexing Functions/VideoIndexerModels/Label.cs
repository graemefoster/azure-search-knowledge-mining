using Newtonsoft.Json;

namespace Azure.KnowledgeMining
{
    public class Label
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}