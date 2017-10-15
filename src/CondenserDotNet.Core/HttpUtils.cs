using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

namespace CondenserDotNet.Core
{
    public static class HttpUtils
    {
        private static readonly string _indexHeader = "X-Consul-Index";
        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };
        public static readonly string ApiUrl = "/v1/";
        public static readonly string ServiceCatalogUrl = ApiUrl + "catalog/services";
        public static readonly string DatacenterCatalogUrl = ApiUrl + "catalog/datacenters";
        public static readonly string SingleServiceCatalogUrl = ApiUrl + "catalog/service/";
        public static readonly string ServiceHealthUrl = ApiUrl + "health/service/";
        public static readonly string SessionCreateUrl = ApiUrl + "session/create";
        public static readonly string HealthAnyUrl = ApiUrl + "health/state/any";

        public static readonly string DefaultHost = "localhost";
        public static readonly int DefaultPort = 8500;
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(6);

        public static StringContent GetStringContent<T>(T objectForContent) => new StringContent(JsonConvert.SerializeObject(objectForContent, JsonSettings), Encoding.UTF8, "application/json");

        public static HttpClient CreateClient(string agentHost = null, int? agentPort = null)
        {
            var host = agentHost ?? DefaultHost;
            var port = agentPort ?? DefaultPort;

            var uri = new UriBuilder("http", host, port);
#if NET452
            System.Net.ServicePointManager.DefaultConnectionLimit = 50;
            return new HttpClient()
            {
                BaseAddress = uri.Uri,
                Timeout = DefaultTimeout
            };
#else
            return new HttpClient(new HttpClientHandler() { MaxConnectionsPerServer = 50 })
            {
                BaseAddress = uri.Uri,
                Timeout = DefaultTimeout
            };
#endif
        }

        public static Task<T> GetObject<T>(this HttpContent content) =>
        content.ReadAsStringAsync().ContinueWith(sTask =>
        {
            return JsonConvert.DeserializeObject<T>(sTask.Result);
        });

        public static Task<T> GetAsync<T>(this HttpClient client, string uri) =>
        client.GetStringAsync(uri).ContinueWith(resultTask =>
        {
            return JsonConvert.DeserializeObject<T>(resultTask.Result);
        });

        public static StringContent GetStringContent(string stringForContent)
        {
            if (stringForContent == null)
            {
                return null;
            }
            var returnValue = new StringContent(stringForContent, Encoding.UTF8);
            return returnValue;
        }

        public static string GetConsulIndex(this HttpResponseMessage response)
        {
            if (!response.Headers.TryGetValues(_indexHeader, out var results))
            {
                return "0";
            }
            return results.FirstOrDefault();
        }

        public static string StripFrontAndBackSlashes(string inputString)
        {
            var startIndex = inputString.StartsWith("/") ? 1 : 0;
            return inputString.Substring(startIndex, (inputString.Length - startIndex) - (inputString.EndsWith("/") ? 1 : 0));
        }
    }
}
