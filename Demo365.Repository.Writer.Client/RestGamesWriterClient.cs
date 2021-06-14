using Demo365.Contracts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Demo365.Repository.Writer.Client
{
    public class RestGamesWriterClient : IGamesSyncWriterClient
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _basepath;
        private readonly ILogger<RestGamesWriterClient> _logger;

        public RestGamesWriterClient(string basepath, ILogger<RestGamesWriterClient> logger)
        {
            _basepath = basepath;
            _logger = logger;
        }

        public async Task<AddResult> WriteAsync(AddRequest request)
        {
            var url = $"{_basepath}/games";
            try
            {
                _logger.LogInformation($"Sending data to {url}, total = {request.Items.Count()}");

                var searchJson = JsonConvert.SerializeObject(request);
                var httpContent = new StringContent(searchJson, Encoding.UTF8, "application/json");

                var httpResponse = await _httpClient.PostAsync(url, httpContent);
                var resultJson = await httpResponse.Content.ReadAsStringAsync();

                var addResult = JsonConvert.DeserializeObject<AddResult>(resultJson);
                _logger.LogInformation($"Sent data to {url}, total = {request.Items.Count()}");
                return addResult;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, $"Could not send data to {url}");
                throw;
            }
        }
    }
}
