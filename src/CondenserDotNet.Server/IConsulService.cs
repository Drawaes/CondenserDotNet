using System.Threading.Tasks;

namespace CondenserDotNet.Server
{
    public interface IConsulService : IService
    {
        Task Initialise(string serviceId, string nodeId, string[] tags, string address, int port);
    }
}