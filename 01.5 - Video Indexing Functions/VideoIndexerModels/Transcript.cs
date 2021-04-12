using Newtonsoft.Json;

namespace Azure.KnowledgeMining
{
    public class Transcript
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}