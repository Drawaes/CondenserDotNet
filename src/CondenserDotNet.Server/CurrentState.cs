using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using CondenserDotNet.Server.DataContracts;

namespace CondenserDotNet.Server
{
    public class CurrentState : ICurrentState
    {
        private DateTime _startedTime;

        public CurrentState() => _startedTime = DateTime.UtcNow;

        public StatsSummary GetSummary()
        {
            var stats = new StatsSummary();

            lock (_lock)
            {
                UpTime = DateTime.UtcNow - _startedTime;

                stats.Http100Responses = Http100Responses;
                stats.Http200Responses = Http200Responses;
                stats.Http300Responses = Http300Responses;
                stats.Http400Responses = Http400Responses;
                stats.Http500Responses = Http500Responses;
                stats.HttpUnknownResponse = HttpUnknownResponse;
                stats.UpTime = UpTime;
            }
            return stats;
        }

        public int Http100Responses { get; private set; }
        public int Http200Responses { get; private set; }
        public int Http300Responses { get; private set; }
        public int Http400Responses { get; private set; }
        public int Http500Responses { get; private set; }
        public TimeSpan UpTime { get; private set; }
        public int HttpUnknownResponse { get; private set; }

        private readonly object _lock = new object();

        public void RecordResponse(int responseCode)
        {
            lock (_lock)
            {
                responseCode = responseCode / 100;
                switch (responseCode)
                {
                    case 5:
                        Http500Responses++;
                        break;
                    case 4:
                        Http400Responses++;
                        break;
                    case 3:
                        Http300Responses++;
                        break;
                    case 2:
                        Http200Responses++;
                        break;
                    case 1:
                        Http100Responses++;
                        break;
                    default:
                        HttpUnknownResponse++;
                        break;
                }
            }

        }

        public void ResetUptime() => _startedTime = DateTime.UtcNow;
    }
}
