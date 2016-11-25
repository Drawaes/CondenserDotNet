using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondenserDotNet.Server.Consul
{
    /// <summary>
    /// Primary watcher of service health status and keeps a list of services up to date
    /// </summary>
    public class ConsulWatcher
    {
        public ConsulWatcher()
            :this("localhost",8500)
        {

        }

        public ConsulWatcher(string agentaddress, int port)
        {

        }
    }
}
