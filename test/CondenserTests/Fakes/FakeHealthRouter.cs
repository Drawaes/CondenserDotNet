using System.Threading.Tasks;
using CondenserDotNet.Server;
using Microsoft.AspNetCore.Http;

namespace CondenserTests.Fakes
{
    public class FakeHealthRouter : IHealthRouter
    {
        public string Route { get; }

        public Task CheckHealth(HttpContext context)
        {
            return Task.FromResult(0);
        }
    }
}