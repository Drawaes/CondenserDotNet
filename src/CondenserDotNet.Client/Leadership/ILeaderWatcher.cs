using System.Threading.Tasks;
using CondenserDotNet.Core.DataContracts;

namespace CondenserDotNet.Client.Leadership
{
    public interface ILeaderWatcher
    {
        Task<InformationService> GetCurrentLeaderAsync();
        Task GetLeadershipAsync();
    }
}