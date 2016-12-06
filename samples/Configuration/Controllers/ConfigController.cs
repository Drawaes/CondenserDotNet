using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Configuration.Controllers
{
    [Route("[controller]")]
    public class ConfigController : Controller
    {
        private readonly IOptions<ConsulConfig> _config;

        public ConfigController(IOptions<ConsulConfig> config)
        {
            _config = config;
        }

        public IActionResult Get()
        {
            return Ok("Config: " + _config.Value.Setting);
        }
    }
}