using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Azure.KnowledgeMining
{
    public static class EndVideoIndexing
    {
        [FunctionName("EndVideoIndexing")]
        public static async Task<string> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "EndVideoIndexing/")]
            HttpRequest req,
            Binder binder,
            ILogger log)
        {
            var indexerId = req.Query["id"];
            var indexerState = req.Query["state"];

            if (indexerState[0] != "Processed")
            {
                log.LogWarning("Callback from indexer. Id:{Id}. State from indexer:{State}", indexerId, indexerState);
                return String.Empty;
            }

            var indexerInsights = await GetIndexerInsights(log, indexerId);

            using (var writer = await binder.BindAsync<TextWriter>(new BlobAttribute($"video-index-result/{indexerId}.json")))
            {
                log.LogInformation("Indexer result:");
                log.LogInformation(indexerInsights);
                await writer.WriteAsync(indexerInsights);
                await writer.FlushAsync();
            }

            return indexerId;
        }
        
        private static async Task<string> GetIndexerInsights(ILogger logger, string videoId)
        {
            var endpoint = "https://api.videoindexer.ai";
            var accountId = Environment.GetEnvironmentVariable("MediaIndexer_AccountId");
            var location = Environment.GetEnvironmentVariable("MediaIndexer_Location");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("MediaIndexer_AccountKey"));

            var accessToken = JsonConvert.DeserializeObject<string>(await httpClient.GetStringAsync($"{endpoint}/auth/{location}/Accounts/{accountId}/AccessToken?allowEdit=true"));
            logger.LogInformation("Retrieved access token {Token}", accessToken);

            var response = await httpClient.GetStringAsync($"{endpoint}/{location}/Accounts/{accountId}/Videos/{videoId}/Index?accessToken={accessToken}");
            logger.LogInformation("Retrieved insights for video Id:{Id}", videoId);

            return response;
        }

    }
}