using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CondenserTests.Fakes
{
    [Route("[controller]")]
    public class FakeController : Controller
    {
        private readonly IOptions<FakeConfig> _config;

        public FakeController(IOptions<FakeConfig> config) => _config = config;

        public IActionResult Get() => Ok("Config: " + _config.Value.Setting1);

        [Route("fake/route")]
        public IActionResult GetFakeRoute() => Ok("Was routed");
    }
}
