using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ServiceRegistration.Controllers
{
    [Route("[controller]")]
    public class HealthController:Controller
    {
        public IActionResult Get()
        {
            return Ok("Test");
        }
    }
}
