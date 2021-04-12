using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.KnowledgeMining.SearchModels;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
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
            var videoIndexerResult = JsonConvert.DeserializeObject<VideoIndexerResult>(indexerInsights);
            var searchIndexModel = TransformVideoIndexModelToSearchIndexModel(videoIndexerResult);
            var searchIndexJson = JsonConvert.SerializeObject(searchIndexModel);
            await UploadSearchIndexModel(log, videoIndexerResult.Name, indexerId, searchIndexModel);

            using (var writer = await binder.BindAsync<TextWriter>(new BlobAttribute($"video-index-result/{indexerId}.json")))
            {
                await writer.WriteAsync(indexerInsights);
                await writer.FlushAsync();
            }

            using (var writer = await binder.BindAsync<TextWriter>(new BlobAttribute($"video-index-result/{indexerId}-searchModel.json")))
            {
                await writer.WriteAsync(searchIndexJson);
                await writer.FlushAsync();
            }

            return indexerId;
        }

        /// <summary>
        /// Load the model into the search index
        /// </summary>
        private static async Task UploadSearchIndexModel(ILogger logger, string documentName, string documentKey, SearchIndexEntry searchIndexModel)
        {
            var searchService = Environment.GetEnvironmentVariable("MediaIndexer_SearchServiceName");
            var searchIndex = Environment.GetEnvironmentVariable("MediaIndexer_SearchIndexName");
            var searchApiKey = Environment.GetEnvironmentVariable("MediaIndexer_SearchApiKey");
            
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("api-key", searchApiKey);
            var indexRequest = new { value = new[] {searchIndexModel.ToSearchIndexUpload($"indexed-video/{documentKey}", documentName)} };
            
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://{searchService}.search.windows.net/indexes/{searchIndex}/docs/index?api-version=2020-06-30")
            {
                Content = new StringContent(JsonConvert.SerializeObject(indexRequest), Encoding.UTF8, "application/json")
            };
            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Successfully added document to index");
            }
            else
            {
                logger.LogWarning("Failed to add document to index. Code:{StatusCode}, Reason:{Reason}", response.StatusCode, response.ReasonPhrase);
            }
        }

        private static SearchIndexEntry TransformVideoIndexModelToSearchIndexModel(VideoIndexerResult indexerInsights)
        {
            return new SearchIndexEntry()
            {
                Content = string.Join(" ", indexerInsights.Videos.SelectMany(video => video.Insights.Transcript.Select(transcript => transcript.Text))),
                Persons = indexerInsights.SummarizedInsights.Faces.Select(face => face.Name).ToArray(),
                Organizations = Array.Empty<string>(),
                Locations = indexerInsights.SummarizedInsights.NamedLocations.Select(x => x.Name).ToArray(), 
                KeyPhrases = indexerInsights.SummarizedInsights.Keywords.Select(x => x.Name)
                    .Union(indexerInsights.SummarizedInsights.Labels.Select(x => x.Name))
                    .Union(indexerInsights.SummarizedInsights.Topics.Select(x => x.Name))
                    .Union(indexerInsights.SummarizedInsights.Sentiments.Select(x => x.SentimentKey))
                    .Union(indexerInsights.SummarizedInsights.Emotions.Select(x => x.Type))
                    .ToArray()
            };
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

            var response = await httpClient.GetStringAsync($"{endpoint}/{location}/Accounts/{accountId}/Videos/{videoId}/Index?includeStreamingUrls=True&accessToken={accessToken}");
            logger.LogInformation("Retrieved insights for video Id:{Id}", videoId);

            return response;
        }

    }
}