using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Text.Formatting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CondenserDotNet.Server.HttpPipelineClient
{
    public static class HttpRequestHelper
    {
        private static readonly Task _cachedTask = Task.FromResult(0);

        public static WritableBufferAwaitable WriteHeadersAsync(this IPipeConnection connection, HttpContext context,string host)
        {
            var writer = connection.Output.Alloc();
            writer.Append(context.Request.Method, TextEncoder.Utf8);
            writer.Write(HttpConsts.Space);
            writer.Append(context.Request.Path.Value, TextEncoder.Utf8);
            writer.Write(HttpConsts.Space);
            writer.Append(context.Request.Protocol, TextEncoder.Utf8);
            writer.Write(HttpConsts.EndOfLine);
            foreach (var header in context.Request.Headers)
            {
                if (header.Key == "Host")
                {
                    writer.Append(header.Key, TextEncoder.Utf8);
                    writer.Write(HttpConsts.HeaderSplit);
                    writer.Append(host, TextEncoder.Utf8);
                    writer.Write(HttpConsts.EndOfLine);
                    continue;
                }

                writer.Append(header.Key, TextEncoder.Utf8);
                writer.Write(HttpConsts.HeaderSplit);
                writer.Append(string.Join(", ", header.Value), TextEncoder.Utf8);
                writer.Write(HttpConsts.EndOfLine);
            }
            writer.Write(HttpConsts.EndOfLine);
            return writer.FlushAsync();
        }

        public static Task WriteBodyAsync(this IPipeConnection connection, HttpContext context)
        {
            if (context.Request.Headers["Transfer-Encoding"] == "chunked")
            {
                throw new NotImplementedException();
            }
            if (context.Request.ContentLength > 0)
            {
                throw new NotImplementedException();
            }
            return _cachedTask;
        }
    }
}
