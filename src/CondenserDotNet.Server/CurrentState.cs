using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Server
{
    public class CurrentState
    {
        private readonly System.Threading.ThreadLocal<ThreadStats> _stats 
            = new System.Threading.ThreadLocal<ThreadStats>(() => new ThreadStats(), true);
        private readonly ILogger<CurrentState> _logger;
        private readonly DateTime _startedTime;

        internal class ThreadStats
        {
            public int Http100Responses;
            public int Http200Responses;
            public int Http300Responses;
            public int Http400Responses;
            public int Http500Responses;
            public int HttpUnknownResponse;
        }

        public CurrentState(ILoggerFactory logger)
        {
            _logger = logger?.CreateLogger<CurrentState>();
            _startedTime = DateTime.UtcNow;
        }

        internal ThreadStats Stats => _stats.Value;

        public Summary GetSummary()
        {
            var returnValue = new Summary();
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

        public struct Summary
        {
            public int Http100Responses;
            public int Http200Responses;
            public int Http300Responses;
            public int Http400Responses;
            public int Http500Responses;
            public TimeSpan UpTime;
            public int HttpUnknownResponse;
        }

        public void RecordResponse(int responseCode)
        {
            switch(responseCode)
            {
                case 500:
                    Stats.Http500Responses++;
                    break;
                case 400:
                    Stats.Http400Responses++;
                    break;
                case 300:
                    Stats.Http300Responses++;
                    break;
                case 200:
                    Stats.Http200Responses++;
                    break;
                case 100:
                    Stats.Http100Responses++;
                    break;
                default:
                    Stats.HttpUnknownResponse++;
                    break;
            }
            
        }
    }
}
