using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CondenserDotNet.Client.Internal
{
    internal static class HttpUtils
    {
        private readonly static JsonSerializerSettings _jsonSettings;
        private readonly static string _indexHeader = "X-Consul-Index";
        public readonly static string ApiUrl = "/v1/";
        public readonly static string KeyUrl = ApiUrl + "kv/";
        public readonly static string ServiceCatalogUrl = ApiUrl + "catalog/services";
        public readonly static string DatacenterCatalogUrl = ApiUrl + "catalog/datacenters";


        static HttpUtils()
        {
            _jsonSettings = new JsonSerializerSettings { ContractResolver = new DefaultContractResolver(), NullValueHandling = NullValueHandling.Ignore };
        }
        public static StringContent GetStringContent<T>(T objectForContent)
        {
            var returnValue = new StringContent(JsonConvert.SerializeObject(objectForContent, _jsonSettings), Encoding.UTF8, "application/json");
            return returnValue;
        }
        public static StringContent GetStringContent(string stringForContent)
        {
            var returnValue = new StringContent(stringForContent, Encoding.UTF8);
            return returnValue;
        }

        public static string GetConsulIndex(this HttpResponseMessage response)
        {
            IEnumerable<string> results;
            if (!response.Headers.TryGetValues(_indexHeader, out results))
            {
                return string.Empty;
            }
            return results.FirstOrDefault();
        }
    }
}
