using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CondenserDotNet.Client.Leadership
{
    public class LeaderRegistry : ILeaderRegistry
    {
        private readonly IServiceManager _serviceManager;
        private readonly Dictionary<string, LeaderWatcher> _leaderWatchers = new Dictionary<string, LeaderWatcher>(StringComparer.OrdinalIgnoreCase);

        public LeaderRegistry(IServiceManager serviceManager) => _serviceManager = serviceManager;

        public async Task<ILeaderWatcher> GetLeaderWatcherAsync(string keyForLeadership)
        {
            var registrationTask = _serviceManager.RegistrationTask;
            if(registrationTask == null)
            {
                throw new InvalidOperationException($"You need to register your service before you can apply for leadership locks lock attempted {keyForLeadership}");
            }
            await registrationTask;
            lock (_leaderWatchers)
            {
                if (!_leaderWatchers.TryGetValue(keyForLeadership, out var returnValue))
                {
                    returnValue = new LeaderWatcher(_serviceManager, keyForLeadership);
                    _leaderWatchers[keyForLeadership] = returnValue;
                }
                return returnValue;
            }
        }
    }
}
