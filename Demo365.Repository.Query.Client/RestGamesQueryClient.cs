using Demo365.Contracts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Demo365.Repository.Query.Client
{
    public class RestGamesQueryClient : IGamesQueryClient
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _basepath;
        private readonly ILogger<RestGamesQueryClient> _logger;

        public RestGamesQueryClient(string basepath, ILogger<RestGamesQueryClient> logger)
        {
            _basepath = basepath;
            _logger = logger;
        }

        public async Task<SearchResult> SearchAsync(SearchRequest search)
        {
            var url = $"{_basepath}/games/search";

            try
            {
                _logger.LogDebug($"Searching for [{search}] / {url}");

                var searchJson = JsonConvert.SerializeObject(search);
                var httpContent = new StringContent(searchJson, Encoding.UTF8, "application/json");

                var httpResponse = await _httpClient.PostAsync(url, httpContent);
                var resultJson = await httpResponse.Content.ReadAsStringAsync();

                var searchResult = JsonConvert.DeserializeObject<SearchResult>(resultJson);
                _logger.LogInformation($"Found total = {searchResult.Items.Count()} for [{search}] / {url}");
                return searchResult;
            }
            catch (Exception ex) 
            {
                _logger.LogInformation(ex, $"Failed to search [{search}] / {url}");
                throw;
            }
        }
    }
}
