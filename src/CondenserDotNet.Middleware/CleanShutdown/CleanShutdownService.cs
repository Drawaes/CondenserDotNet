using Microsoft.Extensions.Logging;
using System.Threading;

namespace CondenserDotNet.Middleware.CleanShutdown
{
    public class CleanShutdownService
    {
        private readonly CountdownEvent _requestsOutstanding = new CountdownEvent(1);
        private readonly int _shutdownTimeout = 2000;
        private readonly ILogger _logger;

        public CleanShutdownService(ILoggerFactory loggerFactory) => _logger = loggerFactory.CreateLogger<CleanShutdownService>();
        public void StartRequest() => _requestsOutstanding.AddCount();
        public void FinishRequest() => _requestsOutstanding.Signal();

        public void Shutdown()
        {
            _requestsOutstanding.Signal();
            if (!_requestsOutstanding.Wait(_shutdownTimeout))
            {
                _logger?.LogWarning($"Could not perform a clean shutdown there were {_requestsOutstanding.CurrentCount} remaining when the {_shutdownTimeout}ms timeout expired");
            }
            else
            {
                _logger?.LogInformation("Clean shutdown completed succesfully");
            }
        }
    }
}
