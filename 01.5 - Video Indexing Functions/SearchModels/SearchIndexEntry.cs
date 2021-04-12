using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Azure.KnowledgeMining.SearchModels
{
    public class SearchIndexEntry
    {
        public string Content { get; set; }

        public string[] KeyPhrases { get; set; }

        public string[] Organizations { get; set; }

        public string[] Persons { get; set; }

        public string[] Locations { get; set; }

        public Dictionary<string, object> ToSearchIndexUpload(string documentPath, string documentName)
        {
            return new Dictionary<string, object>()
            {
                //{"@search.action", "delete"}, //mergeOrUpload"},
                {"@search.action", "mergeOrUpload"},
                {
                    "metadata_storage_path", UrlSafeBase64(documentPath)
                },
                {"metadata_storage_name", documentName},
                {"content", Content},
                {"keyPhrases", KeyPhrases},
                {"organizations", Organizations},
                {"locations", Locations},
                {"persons", Persons}
            };
        }

        private static string UrlSafeBase64(string documentPath)
        {
            var interimBase64 = Convert.ToBase64String(Encoding.Default.GetBytes(documentPath))
                .Replace("+", "-")
                .Replace("/", "_");
            var paddingCharacters = interimBase64.Count(x => x == '=');
            return $"{interimBase64.Substring(0, interimBase64.Length - paddingCharacters)}{paddingCharacters}";
        }
    }
}