using Newtonsoft.Json;

namespace Azure.KnowledgeMining
{
    public class Keyword
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}