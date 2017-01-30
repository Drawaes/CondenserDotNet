using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CondenserDotNet.Server.Routes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CondenserDotNet.Server
{
    public class Service : IDisposable, IService
    {

        private readonly HttpClient _httpClient;
        private readonly System.Threading.CountdownEvent _waitUntilRequestsAreFinished = new System.Threading.CountdownEvent(1);
        private readonly string _address;
        private readonly int _port;
        private const string UrlPrefix = "urlprefix-";
        private CurrentState _stats;

        public Service()
        {
        }
        public Service(string serviceId,  
            string nodeId, string[] tags,
            string address, int port, CurrentState stats)
        {
            _stats = stats;
            _httpClient = new HttpClient(new HttpClientHandler());
            _address = address;
            _port = port;
            Tags = tags;
            Routes = RoutesFromTags(tags);
            ServiceId = serviceId;
            NodeId = nodeId;
            
            SupportedVersions = tags.Where(t => t.StartsWith("version=")).Select(t => new Version(t.Substring(8))).ToArray();
        }

        public Version[] SupportedVersions { get; private set; }
        public string[] Tags { get; private set; }
        public string[] Routes { get; private set; }
        public string ServiceId { get; private set; }
        public string NodeId { get; private set; }

        public static string[] RoutesFromTags(string[] tags)
        {
            int returnCount = 0;
            for(int i = 0; i < tags.Length;i++)
            {
                if(!tags[i].StartsWith(UrlPrefix))
                {
                    continue;
                }
                returnCount ++;
            }
            var returnValues = new string[returnCount];
            returnCount =0;
            for(int i = 0; i < tags.Length; i++)
            {
                if(!tags[i].StartsWith(UrlPrefix))
                {
                    continue;
                }
                var startSubstIndex = UrlPrefix.Length;
                var endSubstIndex = tags[i].Length - UrlPrefix.Length;
                if(tags[i][tags[i].Length -1] == '/')
                {
                    endSubstIndex --;
                }
                returnValues[returnCount] = tags[i].Substring(startSubstIndex, endSubstIndex);
                if(returnValues[returnCount][0] != '/')
                {
                    returnValues[returnCount] = "/" + returnValues[returnCount];
                }
                returnCount++;
            }
            return returnValues;
        }

        public async Task CallService(HttpContext context)
        {
            _waitUntilRequestsAreFinished.AddCount();
            try
            {
                var hostString = $"{_address}:{_port}";

                var routeData = context.GetRouteData();
                string uriString;

                if (routeData != null)
                {
                    var apiPath = (string) routeData.DataTokens["apiPath"];
                    string remainingPath = context.Request.Path.Value.Substring(apiPath.Length);
                    uriString = $"http://{hostString}{remainingPath}{context.Request.QueryString}";
                }
                else
                {
                    uriString = $"http://{hostString}{context.Request.Path.Value}{context.Request.QueryString}";
                }

                var uri = new Uri(uriString);

                var requestMessage = new HttpRequestMessage();
                if (!string.Equals(context.Request.Method, "GET", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(context.Request.Method, "HEAD", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(context.Request.Method, "DELETE", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(context.Request.Method, "TRACE", StringComparison.OrdinalIgnoreCase))
                {
                    var streamContent = new StreamContent(context.Request.Body);
                    requestMessage.Content = streamContent;
                }
                requestMessage.RequestUri = uri;
                requestMessage.Method = new HttpMethod(context.Request.Method);
                // Copy the request headers
                foreach (var header in context.Request.Headers)
                {
                    if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && requestMessage.Content != null)
                    {
                        requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                    }
                }

                requestMessage.Headers.Host = hostString;

                requestMessage.Method = new HttpMethod(context.Request.Method);

                using (var responseMessage = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
                {
                    context.Response.StatusCode = (int)responseMessage.StatusCode;
                    _stats?.RecordResponse(context.Response.StatusCode);
                    foreach (var header in responseMessage.Headers)
                    {
                        context.Response.Headers[header.Key] = header.Value?.ToArray();
                    }

                    foreach (var header in responseMessage.Content.Headers)
                    {
                        context.Response.Headers[header.Key] = header.Value?.ToArray();
                    }

                    // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
                    context.Response.Headers.Remove("transfer-encoding");
                    await responseMessage.Content.CopyToAsync(context.Response.Body);
                }
            }
            finally
            {
                _waitUntilRequestsAreFinished.Signal();
            }
        }

        public override int GetHashCode()
        {
            return ServiceId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var otherService = obj as Service;
            if (otherService != null)
            {
                
                if (otherService.ServiceId != ServiceId)
                {
                    return false;
                }
                if (!Tags.SequenceEqual(otherService.Tags))
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public void UpdateRoutes(string[] routes)
        {
            Routes = routes;
        }

        public void Dispose()
        {
            _waitUntilRequestsAreFinished.Signal();
            _waitUntilRequestsAreFinished.Wait(5000);
            _httpClient.Dispose();
            _waitUntilRequestsAreFinished.Dispose();
        }
    }
}
