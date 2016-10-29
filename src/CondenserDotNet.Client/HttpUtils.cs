using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CondenserDotNet.Client
{
    internal static class HttpUtils
    {
        private readonly static JsonSerializerSettings _jsonSettings;

        static HttpUtils()
        {
            _jsonSettings = new JsonSerializerSettings { ContractResolver = new DefaultContractResolver(), NullValueHandling = NullValueHandling.Ignore };
        }
        public static StringContent GetStringContent<T>(T objectForContent)
        {
            var returnValue = new StringContent(JsonConvert.SerializeObject(objectForContent, _jsonSettings), Encoding.UTF8, "application/json");
            return returnValue;
        }
    }
}
