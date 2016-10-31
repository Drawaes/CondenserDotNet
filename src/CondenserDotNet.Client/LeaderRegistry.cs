using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CondenserDotNet.Client.DataContracts;
using CondenserDotNet.Client.Internal;

namespace CondenserDotNet.Client
{
    public class LeaderRegistry
    {
        private readonly ServiceManager _serviceManager;
        private readonly Dictionary<string, LeaderWatcher> _leaderWatchers = new Dictionary<string, LeaderWatcher>(StringComparer.OrdinalIgnoreCase);

        internal LeaderRegistry(ServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public LeaderWatcher GetLeaderWatcher(string keyForLeadership)
        {
            LeaderWatcher returnValue;
            lock (_leaderWatchers)
            {
                if(!_leaderWatchers.TryGetValue(keyForLeadership, out returnValue))
                {
                    returnValue = new LeaderWatcher(_serviceManager, keyForLeadership);
                    _leaderWatchers[keyForLeadership] = returnValue;
                }
            }
            return returnValue;
        }

    }
}
