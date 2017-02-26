using System;
using System.Collections.Generic;

namespace CondenserDotNet.Client.Leadership
{
    public class LeaderRegistry : ILeaderRegistry
    {
        private readonly IServiceManager _serviceManager;
        private readonly Dictionary<string, LeaderWatcher> _leaderWatchers = new Dictionary<string, LeaderWatcher>(StringComparer.OrdinalIgnoreCase);

        public LeaderRegistry(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public ILeaderWatcher GetLeaderWatcher(string keyForLeadership)
        {
            lock (_leaderWatchers)
            {
                if (!_leaderWatchers.TryGetValue(keyForLeadership, out LeaderWatcher returnValue))
                {
                    returnValue = new LeaderWatcher(_serviceManager, keyForLeadership);
                    _leaderWatchers[keyForLeadership] = returnValue;
                }
                return returnValue;
            }
        }
    }
}
