using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CondenserDotNet.Middleware.TrailingHeaders
{
    public class TrailingHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public TrailingHeadersMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context)
        {
            var trailingHeaders = new TrailingHeadersFeature(context);
            var stream = new ChunkingStream()
            {
                InnerStream = context.Response.Body
            };
            context.Response.Body = stream;
            context.Response.Headers["Transfer-Encoding"] = "chunked";
            context.Features.Set<ITrailingHeadersFeature>(trailingHeaders);
            await _next(context);
            context.Response.Body = stream.InnerStream;
            var sb = new StringBuilder();
            sb.AppendLine("0");
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
