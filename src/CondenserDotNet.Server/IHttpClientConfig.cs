using System.Net.Http;

namespace CondenserDotNet.Server
{
    public interface IHttpClientConfig
    {
        HttpClient Create(string serviceId);
    }
}