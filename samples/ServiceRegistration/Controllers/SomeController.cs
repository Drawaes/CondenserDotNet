using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace ServiceRegistration.Controllers
{
    [Route("/testSample/test3/test1")]
    public class SomeController : Controller
    {
        [HttpGet()]
        public IActionResult GetOther()
        {
            return Ok(Enumerable.Repeat("Quick Brown Fox",100));
        }
    }
}