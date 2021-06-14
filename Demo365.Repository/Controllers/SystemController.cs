using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Demo365.Repository.Controllers
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
