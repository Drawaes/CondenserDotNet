using System;
using System.Linq;
using CondenserDotNet.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace ServiceRegistration.Controllers
{
    [Route("/testSample/test3/test1")]
    public class SomeController : Controller
    {
        private readonly static string[] _content = Enumerable.Repeat("Quick Brown Fox", 100).ToArray();

        [HttpGet()]
        public IActionResult GetOther()
        {
            HttpContext.Features.Get<ITrailingHeadersFeature>().RegisterHeader("TimsHeader", () => DateTime.UtcNow.ToString());
            return Ok(_content);
        }
    }
}