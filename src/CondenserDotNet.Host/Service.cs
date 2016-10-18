using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CondenserDotNet.Host
{
    public class Service:IDisposable
    {
        public Service(string[] routes, string serviceId, string address, int port, string[] tags)
        {
            Tags = tags;
            Routes = routes.Select(r => !r.StartsWith("/") ? "/" + r : r).Select(r => r.EndsWith("/") ? r.Substring(0,r.Length-1) : r).ToArray();
            ServiceId = serviceId;
            Address = address;
            Port = port;
            _hostString = Address + ":" + port;
            SupportedVersions = tags.Where(t => t.StartsWith("version=")).Select(t => new System.Version(t.Substring(8))).ToArray(); 
        }

        private string _hostString;
        private HttpClient _httpClient = new HttpClient(new HttpClientHandler());
        private System.Threading.CountdownEvent _waitUntilRequestsAreFinished = new System.Threading.CountdownEvent(0);

        public System.Version[] SupportedVersions { get; private set;}
        public string[] Tags { get; private set;}
        public int Port { get; private set; }
        public string Address { get; private set; }
        public string[] Routes { get; private set; }
        public string ServiceId { get; private set; }

        private bool disposed = false;

        public async Task CallService(HttpContext context)
        {
            _waitUntilRequestsAreFinished.AddCount();
            try
            {
                var apiPath = (string)context.GetRouteData().DataTokens["apiPath"];

                string remainingPath = context.Request.Path.Value.Substring(apiPath.Length);
                var uriString = $"http://{_hostString}{remainingPath}{context.Request.QueryString}";
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

                requestMessage.Headers.Host = _hostString;

                requestMessage.Method = new HttpMethod(context.Request.Method);

                using (var responseMessage = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
                {
                    context.Response.StatusCode = (int)responseMessage.StatusCode;
                    foreach (var header in responseMessage.Headers)
                    {
                        context.Response.Headers[header.Key] = header.Value.ToArray();
                    }

                    foreach (var header in responseMessage.Content.Headers)
                    {
                        context.Response.Headers[header.Key] = header.Value.ToArray();
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
            if(otherService != null)
            {
                if(otherService.Address != Address)
                    return false;
                if(otherService.Port != Port)
                    return false;
                if(otherService.ServiceId != ServiceId)
                    return false;
                if(!Tags.SequenceEqual(otherService.Tags))
                    return false;

                return true;
            }
            var consulService = obj as Consul.ServiceEntry;
            if(consulService != null)
            {
                if(consulService.Service.ID != ServiceId)
                    return false;
                if(consulService.Service.Port != Port)
                    return false;
                if(consulService.Service.Address != Address)
                    return false;
                if(!consulService.Service.Tags.Where(t => !t.StartsWith("url=")).SequenceEqual(Tags))
                    return false;

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
            _waitUntilRequestsAreFinished.Wait(5000);
            _httpClient.Dispose();
            _waitUntilRequestsAreFinished.Dispose();
        }
    }
}
