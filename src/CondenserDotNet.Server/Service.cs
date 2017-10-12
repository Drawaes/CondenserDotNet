using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CondenserDotNet.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using CondenserDotNet.Server.DataContracts;

namespace CondenserDotNet.Server
{
    public class Service : IDisposable, IConsulService, IUsageInfo
    {
        private HttpClient _httpClient;
        private readonly System.Threading.CountdownEvent _waitUntilRequestsAreFinished = new System.Threading.CountdownEvent(1);
        private string _address;
        private int _port;
        private ICurrentState _stats;
        private readonly Func<string, HttpClient> _clientFactory;
        private IPEndPoint _ipEndPoint;
        private Version[] _supportedVersions;
        private string[] _tags;
        private string _serviceId;
        private int _calls;
        private int _totalRequestTime;
        private DateTime _lastRequest;
        private double _lastRequestTime;
        private string _hostString;
        private readonly ILogger _logger;
        private string _protocolScheme;
        private RoutingData _routingData;

        public Service(Func<string, HttpClient> clientFactory, ILoggerFactory logger, RoutingData routingData)
        {
            _logger = logger?.CreateLogger<Service>();
            _clientFactory = clientFactory;
            _routingData = routingData;
        }

        public Version[] SupportedVersions => _supportedVersions;
        public string[] Tags => _tags;
        public string[] Routes { get; private set; }
        public string ServiceId => _serviceId;
        public string NodeId { get; private set; }
        public int Calls => _calls;
        public double TotalRequestTime => _totalRequestTime;
        public double LastRequestTime => _lastRequestTime;
        public DateTime LastRequest => _lastRequest;
        public IPEndPoint IpEndPoint => _ipEndPoint;
       
        public async Task CallService(HttpContext context)
        {
            var sw = new Stopwatch();
            _waitUntilRequestsAreFinished.AddCount();
            try
            {
                sw.Start();
                var uriString = $"{_protocolScheme}://{_hostString}{context.Request.Path.Value}{context.Request.QueryString}";

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
                sw.Stop();

                var time = (int)sw.Elapsed.TotalMilliseconds;


                System.Threading.Interlocked.Add(ref _totalRequestTime, time);
                System.Threading.Interlocked.Increment(ref _calls);
                System.Threading.Interlocked.Exchange(ref _lastRequestTime, time);
                _lastRequest = DateTime.UtcNow;

                _waitUntilRequestsAreFinished.Signal();
            }
        }

        public override int GetHashCode() => ServiceId.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is Service otherService)
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

        public void UpdateRoutes(string[] routes) => Routes = routes;

        public async Task Initialise(string serviceId, string nodeId, string[] tags, string address, int port)
        {
            _stats = _routingData.GetStats(serviceId);
            _address = address;
            _port = port;
            _tags = tags;
            Routes = ServiceUtils.RoutesFromTags(tags);
            _serviceId = serviceId;
            
            NodeId = nodeId;
            try
            {
                var result = await Dns.GetHostAddressesAsync(address);
                _ipEndPoint = new IPEndPoint(result[0], port);
            }
            catch
            {
                _logger?.LogWarning("Unable to resolve the host address for {address} when adding the service", address);
            }
            _supportedVersions = tags.Where(t => t.StartsWith("version=")).Select(t => new Version(t.Substring(8))).ToArray();
            _protocolScheme = tags.Where(t => t.StartsWith("protocolScheme-"))
                .Select(t => t.Substring("protocolScheme-".Length)).FirstOrDefault() ?? "http";

            _hostString = $"{_address}:{_port}";
            _httpClient = _clientFactory?.Invoke(ServiceId) ?? new HttpClient();
        }

        public override string ToString() => _serviceId;

        public void Dispose()
        {
            _waitUntilRequestsAreFinished.Signal();
            _waitUntilRequestsAreFinished.Wait(5000);
            _httpClient.Dispose();
            _waitUntilRequestsAreFinished.Dispose();
        }

        public StatsSummary GetSummary() => _stats.GetSummary();
    }
}
