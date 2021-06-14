using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Demo365.Api.Controllers
{
    [ApiController]
    [Route("system")]
    public class SystemController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SystemController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("config")]
        public Dictionary<string, string> GetConfigJson()
        {
            return _configuration.AsEnumerable().ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }
}
