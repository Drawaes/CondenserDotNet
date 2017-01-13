using Microsoft.AspNetCore.Mvc;

namespace ServiceRegistration.Controllers
{
    [Route("api/[controller]")]
    public class SomeController : Controller
    {
        [Route("SomeObject")]
        public IActionResult Get()
        {
            return Ok("Some object");
        }

        [Route("SomeOtherObject")]
        public IActionResult GetOther()
        {
            return Ok("Some other object");
        }
    }
}