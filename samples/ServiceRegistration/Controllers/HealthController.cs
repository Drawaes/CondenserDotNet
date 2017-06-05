using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CondenserDotNet.Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace ServiceRegistration.Controllers
{
    [Route("[controller]")]
    public class HealthController:Controller
    {
        private IServiceRegistry _registry;

        public HealthController(IServiceRegistry registry)
        {
            _registry = registry;
        }

        public IActionResult Get()
        {
            var instance = _registry.GetServiceInstanceAsync("TestService");
            instance.Wait();
            if (instance.Result != null)
            {
                return Ok(instance.Result.Service);
            }
            else
            {
                return Ok();
            }
        }
    }
}
