using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CondenserDotNet.Service
{
    public static class HttpUtils
    {
        private static readonly string _indexHeader = "X-Consul-Index";
        public static readonly JsonSerializerSettings JsonSettings;
        public static readonly string ApiUrl = "/v1/";
        public static readonly string KeyUrl = ApiUrl + "kv/";
        public static readonly string ServiceCatalogUrl = ApiUrl + "catalog/services";
        public static readonly string DatacenterCatalogUrl = ApiUrl + "catalog/datacenters";
        public static readonly string ServiceHealthUrl = ApiUrl + "health/service/";
        public static readonly string SessionCreateUrl = ApiUrl + "session/create";

        static HttpUtils()
        {
            JsonSettings = new JsonSerializerSettings { ContractResolver = new DefaultContractResolver(), NullValueHandling = NullValueHandling.Ignore };
        }

        public static StringContent GetStringContent<T>(T objectForContent)
        {
            var returnValue = new StringContent(JsonConvert.SerializeObject(objectForContent, JsonSettings), Encoding.UTF8, "application/json");
            return returnValue;
        }

        public static StringContent GetStringContent(string stringForContent)
        {
            if(stringForContent == null)
            {
                return null;
            }
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

        public static string StripFrontAndBackSlashes(string inputString)
        {
            int startIndex = inputString.StartsWith("/") ? 1 : 0;
            return inputString.Substring(startIndex, (inputString.Length - startIndex) - (inputString.EndsWith("/") ? 1 : 0));
        }
    }
}
