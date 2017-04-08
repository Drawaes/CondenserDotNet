using System;
using System.Collections.Generic;
using System.Text;
using CondenserDotNet.Client;

namespace CondenserDotNet.Middleware.ConsulCleanShutdown
{
    public class ConsulShutdownService
    {
        private IServiceManager _serviceManager;

        public ConsulShutdownService(IServiceManager serviceManager) => _serviceManager = serviceManager;

        public void Stopping()
        {
            _serviceManager.EnableMaintenanceModeAsync(string.Empty).Wait();
        }

        public void Started()
        {
            _serviceManager.DisableMaintenanceModeAsync().Wait();
        }
    }
}
