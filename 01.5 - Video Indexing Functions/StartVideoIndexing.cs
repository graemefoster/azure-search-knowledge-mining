using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Azure.KnowledgeMining
{
    public static class StartVideoIndexing
    {
        [FunctionName("start_video_indexing")]
        public static async Task Run(
            [BlobTrigger("video-drop/{name}", Connection = "kmvgrfstr_STORAGE")]
            ICloudBlob myBlob, 
            string name, 
            ILogger log)
        {
            log.LogInformation("C# Blob trigger function Processed blob\n Name:{Name} \n Size: {Size} Bytes", name, myBlob.Properties.Length);

            if (!name.Contains(".mp4"))
            {
                return;
            }

            log.LogInformation("Start processing {Name}", name);
            var sasKey = CreateSasKeyForVideoIndexerToRetrieveBlob(myBlob);
            var result = await InitiateVideoIndexing(log, sasKey, myBlob);
            log.LogInformation(result);
        }

        private static async Task<string> InitiateVideoIndexing(ILogger logger, string sasKey, ICloudBlob blob)
        {
            var endpoint = "https://api.videoindexer.ai";
            var accountId = Environment.GetEnvironmentVariable("MediaIndexer_AccountId");
            var location = Environment.GetEnvironmentVariable("MediaIndexer_Location");
            var privacy = "Private";

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("MediaIndexer_AccountKey"));

            var accessToken = JsonConvert.DeserializeObject<string>(await httpClient.GetStringAsync($"{endpoint}/auth/{location}/Accounts/{accountId}/AccessToken?allowEdit=true"));
            logger.LogInformation("Retrieved access token {Token}", accessToken);

            var blobUri = Uri.EscapeDataString(blob.Uri.AbsoluteUri + sasKey);
            var response = await httpClient.PostAsync(
                $"{endpoint}/{location}/Accounts/{accountId}/Videos?accessToken={accessToken}&name={blob.Name}&videoUrl={blobUri}&privacy={privacy}", 
                new ByteArrayContent(Array.Empty<byte>())); //"&callbackUrl=http://")

            return await response.Content.ReadAsStringAsync();
        }

        private static string CreateSasKeyForVideoIndexerToRetrieveBlob(ICloudBlob myBlob)
        {
            return myBlob.GetSharedAccessSignature(new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTimeOffset.Now.AddMinutes(10)
            });
        }
    }
}
