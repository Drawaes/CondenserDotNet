using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Server
{
    public class CurrentState
    {
        private System.Threading.ThreadLocal<ThreadStats> _stats = new System.Threading.ThreadLocal<ThreadStats>(() => new ThreadStats(), true);
        private ILogger<CurrentState> _logger;
        private DateTime _startedTime;

        public class ThreadStats
        {
            public int Http100Responses;
            public int Http200Responses;
            public int Http300Responses;
            public int Http400Responses;
            public int Http500Responses;
            public TimeSpan UpTime;
        }

        public CurrentState(ILoggerFactory logger)
        {
            _logger = logger?.CreateLogger<CurrentState>();
            _startedTime = DateTime.UtcNow;
        }

        public ThreadStats Stats => _stats.Value;

        public ThreadStats GetSummary()
        {
            var returnValue = new ThreadStats();
            try
            {
                foreach (var stat in _stats.Values)
                {
                    returnValue.Http100Responses += stat.Http100Responses;
                    returnValue.Http200Responses += stat.Http200Responses;
                    returnValue.Http300Responses += stat.Http300Responses;
                    returnValue.Http400Responses += stat.Http400Responses;
                    returnValue.Http500Responses += stat.Http500Responses;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(0, ex, "Exception while calculating the health summary");
            }
            returnValue.UpTime = DateTime.UtcNow - _startedTime;
            return returnValue;
        }

        public void RecordResponse(int responseCode)
        {
            if (responseCode > 499)
            {
                Stats.Http500Responses++;
                return;
            }
            if (responseCode > 399)
            {
                Stats.Http400Responses++;
                return;
            }
            if (responseCode > 299)
            {
                Stats.Http300Responses++;
                return;
            }
            if (responseCode > 199)
            {
                Stats.Http200Responses++;
                return;
            }
            if (responseCode > 99)
            {
                Stats.Http100Responses++;
                return;
            }
        }
    }
}
