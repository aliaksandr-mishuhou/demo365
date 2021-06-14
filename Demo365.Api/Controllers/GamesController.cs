using System.Linq;
using System.Threading.Tasks;
using Demo365.Api.Services;
using Demo365.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Demo365.Api.Controllers
{
    [ApiController]
    [Route("game")]
    public class GamesController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<GamesController> _logger;

        public GamesController(ISearchService searchService, ILogger<GamesController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        [HttpPost("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SearchResult>> Search(SearchRequest searchRequest)
        {
            if (searchRequest == null || searchRequest.FromTime == null && searchRequest.ToTime == null)
            {
                return BadRequest();
            }

            _logger.LogDebug($"Searching for request = [{searchRequest}]");

            var result = await _searchService.SearchAsync(searchRequest);
            _logger.LogInformation($"Found {result?.Items?.Count()} for request = [{searchRequest}]");

            return Ok(result);
        }
    }
}
