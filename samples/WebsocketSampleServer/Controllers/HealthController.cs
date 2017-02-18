using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace WebsocketSampleServer.Controllers
{
    [Route("[controller]")]
    public class HealthController : Controller
    {
        public IActionResult Get()
        {
            return Ok("Test");
        }
    }
}
