using System;
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

        [HttpGet("search/{sport}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SearchResult>> SearchBySport(
            [FromRoute]string sport, 
            string competition, string team, DateTime? fromTime, DateTime?  toTime) 
        {
            // basic validation for required params
            if (sport == null)
            {
                return BadRequest();
            }

            return await SearchAsync(new SearchRequest 
            {
                Sport = sport,
                Competition = competition,
                Team = team,
                FromTime = fromTime,
                ToTime = toTime
            });
        }

        [HttpGet("search/{sport}/{competition}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SearchResult>> SearchBySportAndCompetition(
            [FromRoute] string sport,
            [FromRoute] string competition, 
            string team, DateTime? fromTime, DateTime? toTime)
        {
            // basic validation for required params
            if (sport == null && competition == null)
            {
                return BadRequest();
            }

            return await SearchAsync(new SearchRequest
            {
                Sport = sport,
                Competition = competition,
                Team = team,
                FromTime = fromTime,
                ToTime = toTime
            });
        }


        private async Task<ActionResult<SearchResult>> SearchAsync(SearchRequest searchRequest)
        {
            _logger.LogDebug($"Searching for request = [{searchRequest}]");

            var result = await _searchService.SearchAsync(searchRequest);
            _logger.LogInformation($"Found {result?.Items?.Count()} for request = [{searchRequest}]");

            return Ok(result);
        }
    }
}
