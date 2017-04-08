using System;
using System.IO.Pipelines;
using System.Text;
using System.Text.Formatting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CondenserDotNet.Middleware.Pipelines
{
    public static class HttpRequestHelper
    {
        private static readonly Task _cachedTask = Task.FromResult(0);

        public static WritableBufferAwaitable WriteHeadersAsync(this IPipeConnection connection, HttpContext context, byte[] host)
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
                if (header.Key == "Host" || header.Key == "Connection") { }
                writer.Append(header.Key, TextEncoder.Utf8);
                writer.Write(HttpConsts.HeaderSplit);
                writer.Append(string.Join(", ", header.Value), TextEncoder.Utf8);
                writer.Write(HttpConsts.EndOfLine);
            }
            writer.Write(host);
            writer.Write(HttpConsts.ConnectionHeaderBytes);
            return writer.FlushAsync();
        }

        public static Task WriteBodyAsync(this IPipeConnection connection, HttpContext context)
        {
            if (context.Request.Headers["Transfer-Encoding"] == "chunked")
            {
                return connection.WriteChunkedBody(context);
            }
            if (context.Request.ContentLength > 0)
            {
                return connection.WriteBody(context);
            }
            return _cachedTask;
        }

        private static async Task WriteBody(this IPipeConnection connection, HttpContext context)
        {
            var bytesToWrite = context.Request.ContentLength.Value;
            while (bytesToWrite > 0)
            {
                var buffer = connection.Output.Alloc(1024);
                if (!buffer.Memory.TryGetArray(out ArraySegment<byte> byteArray))
                {
                    throw new NotImplementedException();
                }
                var byteCount = await context.Request.Body.ReadAsync(byteArray.Array, byteArray.Offset, byteArray.Count);
                buffer.Advance(byteCount);
                bytesToWrite -= byteCount;
                await buffer.FlushAsync();
            }
        }

        private static async Task WriteChunkedBody(this IPipeConnection connection, HttpContext context)
        {
            while (true)
            {
                var buffer = connection.Output.Alloc();
                try
                {
                    buffer.Ensure(1024);
                    var bookMark = buffer.Memory;
                    buffer.Advance(3);
                    buffer.Write(HttpConsts.EndOfLine);
                    if (!buffer.Memory.TryGetArray(out ArraySegment<byte> byteArray))
                    {
                        throw new NotImplementedException();
                    }
                    var bytesWritten = await context.Request.Body.ReadAsync(byteArray.Array, byteArray.Offset, byteArray.Count);
                    if (bytesWritten == 0)
                    {
                        Encoding.ASCII.GetBytes("000").CopyTo(bookMark.Span);
                        buffer.Write(HttpConsts.EndOfLine);
                        return;
                    }
                    Encoding.ASCII.GetBytes($"{bytesWritten:XXX}").CopyTo(bookMark.Span);
                    buffer.Advance(bytesWritten);
                    buffer.Write(HttpConsts.EndOfLine);
                }
                finally
                {
                    await buffer.FlushAsync();
                }
            }
        }
    }
}
