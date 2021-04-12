using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CognitiveSearch.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoController : ControllerBase
    {
        private readonly ILogger<VideoController> _logger;
        private HttpClient _httpClient;
        private string _location;
        private string _accountId;
        private string _accountKey;

        public VideoController(ILogger<VideoController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("VideoIndexer");
            _location = configuration["VideoIndexerLocation"];
            _accountId = configuration["VideoIndexerAccountId"];
            _accountKey = configuration["VideoIndexerAccountKey"];
        }

        [Route("{videoId}/thumbnail")]
        public async Task<IActionResult> ThumbnailImage(string videoId)
        {
            _logger.LogInformation("Generating Thumbnail Uri for image {VideoId}", videoId);
            
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.videoindexer.ai/auth/{_location}/Accounts/{_accountId}/AccessToken?allowEdit=false");
            request.Headers.Add("Ocp-Apim-Subscription-Key", _accountKey);
            var accessToken = JsonConvert.DeserializeObject<string>(await (await _httpClient.SendAsync(request)).Content.ReadAsStringAsync());

            var metadata = (JObject)JsonConvert.DeserializeObject(await _httpClient.GetStringAsync($"https://api.videoindexer.ai/{_location}/Accounts/{_accountId}/Videos/{videoId}/Index?accessToken={accessToken}"));
            var thumbnailId = metadata["summarizedInsights"].Value<string>("thumbnailId");
            return base.File(
                await _httpClient.GetByteArrayAsync($"https://api.videoindexer.ai/{_location}/Accounts/{_accountId}/Videos/{videoId}/Thumbnails/{thumbnailId}?format=Jpeg&accessToken={accessToken}"),
                "image/jpeg");

        }

        [Route("{videoId}/insights")]
        public async Task<IActionResult> VideoInsights(string videoId)
        {
            _logger.LogInformation("Generating Thumbnail Uri for image {VideoId}", videoId);

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.videoindexer.ai/auth/{_location}/Accounts/{_accountId}/Videos/{videoId}/AccessToken?allowEdit=false");
            request.Headers.Add("Ocp-Apim-Subscription-Key", _accountKey);
            var accessToken = JsonConvert.DeserializeObject<string>(await (await _httpClient.SendAsync(request)).Content.ReadAsStringAsync());

            return Redirect($"https://api.videoindexer.ai/{_location}/Accounts/{_accountId}/Videos/{videoId}/InsightsWidget?widgetType=People&widgetType=Sentiments&widgetType=Keywords&widgetType=Search&accessToken={accessToken}");
        }

        [Route("{videoId}/player")]
        public async Task<IActionResult> VideoPlayer(string videoId)
        {
            _logger.LogInformation("Generating Thumbnail Uri for image {VideoId}", videoId);

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.videoindexer.ai/auth/{_location}/Accounts/{_accountId}/Videos/{videoId}/AccessToken?allowEdit=false");
            request.Headers.Add("Ocp-Apim-Subscription-Key", _accountKey);
            var accessToken = JsonConvert.DeserializeObject<string>(await (await _httpClient.SendAsync(request)).Content.ReadAsStringAsync());
            
            return Redirect($"https://api.videoindexer.ai/{_location}/Accounts/{_accountId}/Videos/{videoId}/PlayerWidget?accessToken={accessToken}");
        }

    }
}