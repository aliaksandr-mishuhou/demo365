using System.Linq;
using System.Threading.Tasks;
using Demo365.Contracts;
using Demo365.Repository.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Demo365.Repository.Controllers
{
    [ApiController]
    [Route("games")]
    public class GamesController : ControllerBase
    {
        private readonly ILogger<GamesController> _logger;
        private readonly IGamesRepository _gamesRepository;

        public GamesController(ILogger<GamesController> logger, IGamesRepository gamesRepository)
        {
            _logger = logger;
            _gamesRepository = gamesRepository;
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

            var result = await _gamesRepository.SearchAsync(searchRequest);

            _logger.LogInformation($"Found {result.Count()} for request = [{searchRequest}]");

            return Ok(new SearchResult { Items = result });
        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> Add(AddRequest addRequest)
        {
            var items = addRequest.Items;
            if (items == null || !items.Any()) 
            {
                return BadRequest();
            }

            _logger.LogDebug($"Adding {items.Count()} games (sync flow)");

            var result = await _gamesRepository.AddAsync(items);

            _logger.LogInformation($"Added {result} games (sync flow)");

            return Ok(new AddResult { Added = result });
        }
    }
}
