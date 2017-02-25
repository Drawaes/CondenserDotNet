using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace CondenserDotNet.Services.Internal
{
    public static class HttpUtils
    {
        private const string IndexHeader = "X-Consul-Index";

        public static string GetConsulIndex(this HttpResponseMessage response)
        {
            if (!response.Headers.TryGetValues(IndexHeader, out IEnumerable<string> results))
            {
                return string.Empty;
            }
            return results.FirstOrDefault();
        }
    }
}
