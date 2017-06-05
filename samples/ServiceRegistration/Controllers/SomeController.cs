using System;
using System.Linq;
using CondenserDotNet.Client.Services;
using CondenserDotNet.Middleware.TrailingHeaders;
using Microsoft.AspNetCore.Mvc;

namespace ServiceRegistration.Controllers
{
    [Route("test")]
    public class SomeController : Controller
    {
        private IServiceRegistry _registry;

        public SomeController(IServiceRegistry registry)
        {
            _registry = registry;
        }

        [HttpGet()]
        public IActionResult GetOther()
        {
            var instance =_registry.GetServiceInstanceAsync("ServiceRegistration");
            instance.Wait();
            return Ok(instance.Result.Service);
        }
    }
}