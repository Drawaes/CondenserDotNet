using System.Threading.Tasks;
using CondenserDotNet.Service.DataContracts;

namespace CondenserDotNet.Client.Internal
{
    public interface ILeaderWatcher
    {
        Task<InformationService> GetCurrentLeaderAsync();
        Task GetLeadershipAsync();
    }
}