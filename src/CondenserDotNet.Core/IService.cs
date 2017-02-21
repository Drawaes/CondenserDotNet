using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CondenserDotNet.Core
{
    public interface IService
    {
        Version[] SupportedVersions { get; }
        string[] Tags { get; }
        string[] Routes { get; }
        string ServiceId { get; }
        string NodeId { get; }
        Task CallService(HttpContext context);
        IPEndPoint IpEndPoint { get; }
        void UpdateRoutes(string[] routes);
    }
}