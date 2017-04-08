using System.Threading.Tasks;
using CondenserDotNet.Core.DataContracts;
using System;

namespace CondenserDotNet.Client.Leadership
{
    public interface ILeaderWatcher
    {
        Task<InformationService> GetCurrentLeaderAsync();
        Task GetLeadershipAsync();
        void SetLeaderCallback(Action<InformationService> callback);
    }
}