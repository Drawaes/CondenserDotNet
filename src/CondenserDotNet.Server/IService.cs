using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CondenserDotNet.Server
{
    public interface IService
    {
        Version[] SupportedVersions { get; }
        string[] Tags { get; }
        string[] Routes { get; }
        string ServiceId { get; }
        string NodeId { get; }
        Task CallService(HttpContext context);
        void UpdateRoutes(string[] routes);
    }
}