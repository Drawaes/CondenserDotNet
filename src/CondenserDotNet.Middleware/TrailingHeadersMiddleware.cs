using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Internal.Http;

namespace CondenserDotNet.Middleware
{
    public class TrailingHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly byte[] s_endOfChunkedBytes = Encoding.ASCII.GetBytes("0\r\n");
        private static readonly byte[] s_seperator = Encoding.ASCII.GetBytes(": ");
        private static readonly byte[] s_end = Encoding.ASCII.GetBytes("\r\n");

        public TrailingHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var trailingHeaders = new TrailingHeadersFeature(context);
            var originalBody = context.Response.Body;
            //context.Response.Headers["Transfer-Encoding"] = "chunked";

            context.Features.Set<ITrailingHeadersFeature>(trailingHeaders);
            await _next(context);
            foreach(var feature in context.Features)
            {
                Console.WriteLine(feature.ToString());
            }
            if(originalBody != context.Response.Body)
            {
                throw new NotImplementedException();
            }

            if (trailingHeaders.Headers.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("\r\n0");
                foreach (var header in trailingHeaders.Headers)
                {
                    sb.Append(header.Item1)
                        .Append(": ")
                        .Append(header.Item2())
                        .AppendLine();
                }
                sb.AppendLine();
                var bytes = Encoding.ASCII.GetBytes(sb.ToString());
                await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
            }
        }
    }
}
