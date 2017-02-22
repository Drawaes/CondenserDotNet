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
        public static WritableBufferAwaitable WriteHeadersAsync(this IPipeConnection connection, HttpContext context)
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
                writer.Append(header.Key, TextEncoder.Utf8);
                writer.Write(HttpConsts.HeaderSplit);
                writer.Append(string.Join(", ", header.Value), TextEncoder.Utf8);
                writer.Write(HttpConsts.EndOfLine);
            }
            writer.Write(HttpConsts.EndOfLine);
            return writer.FlushAsync();
        }
    }
}
