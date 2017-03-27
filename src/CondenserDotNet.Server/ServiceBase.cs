using System;
using System.Net;
using System.Threading.Tasks;
using CondenserDotNet.Server.DataContracts;
using Microsoft.AspNetCore.Http;

namespace CondenserDotNet.Server
{
    public abstract class ServiceBase : IService
    {
        public Version[] SupportedVersions { get; }
        public string[] Tags { get; }
        public abstract string[] Routes { get; }
        public string ServiceId { get; }
        public string NodeId { get; }
        public abstract Task CallService(HttpContext context);
        public abstract IPEndPoint IpEndPoint { get; }
        public abstract bool RequiresAuthentication { get; }

        public virtual void UpdateRoutes(string[] routes)
        {
        }       

        public StatsSummary GetSummary()
        {
            return default(StatsSummary);
        }

        public CurrentState.Summary GetSummary()
        {
            return default(CurrentState.Summary);
        }
    }
}
