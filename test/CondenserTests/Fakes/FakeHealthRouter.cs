using System.Threading.Tasks;
using CondenserDotNet.Server;
using Microsoft.AspNetCore.Http;

namespace CondenserTests.Fakes
{
    public class FakeHealthRouter : ServiceBase
    {
        public FakeHealthRouter(string route)
        {
            Routes = new [] { route};
        }

        public override string[] Routes { get; }

        public override Task CallService(HttpContext context)
        {
            return Task.FromResult(0);
        }
    }
}