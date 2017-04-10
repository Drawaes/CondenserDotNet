using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace CondenserDotNet.Server.Extensions
{
    public static class HttpResponseExtensions
    {
        public static Task WriteJsonAsync<T>(this HttpResponse self, T item) => self.WriteAsync(JsonConvert.SerializeObject(item));
    }
}
