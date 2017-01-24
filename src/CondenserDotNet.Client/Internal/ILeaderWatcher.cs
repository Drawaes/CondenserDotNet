using System.Threading.Tasks;
using CondenserDotNet.Core.DataContracts;

namespace CondenserDotNet.Client.Internal
{
    public interface ILeaderWatcher
    {
        Task<InformationService> GetCurrentLeaderAsync();
        Task GetLeadershipAsync();
    }
}