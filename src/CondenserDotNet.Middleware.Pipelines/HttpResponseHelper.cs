using System;
using System.IO.Pipelines;
using System.IO.Pipelines.Text.Primitives;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Middleware.Pipelines
{
    public static class HttpResponseHelper
    {
        private static readonly Task<bool> _cachedTask = Task.FromResult(false);

        public static async Task ReceiveHeaderAsync(this IPipeConnection connection, HttpContext context)
        {
            var finished = false;
            while (true)
            {
                var reader = await connection.Input.ReadAsync();
                var buffer = reader.Buffer;
                try
                {
                    if (!buffer.TrySliceTo(HttpConsts.HeadersEnd, out ReadableBuffer currentSlice, out ReadCursor cursor))
                    {
                        continue;
                    }
                    buffer = buffer.Slice(cursor).Slice(HttpConsts.HeadersEnd.Length);
                    //first line
                    if (!currentSlice.TrySliceTo(HttpConsts.EndOfLine, out ReadableBuffer currentLine, out cursor))
                    {
                        throw new InvalidOperationException();
                    }
                    context.Response.StatusCode = int.Parse(currentLine.Split(HttpConsts.Space[0]).Skip(1).First().GetAsciiString());
                    currentSlice = currentSlice.Slice(cursor).Slice(HttpConsts.EndOfLine.Length);

                    while (currentSlice.Length > 0)
                    {
                        cursor = ProcessHeader(context, ref currentSlice);
                    }
                    finished = true;
                    return;
                }
                finally
                {
                    if (finished)
                    {
                        connection.Input.Advance(buffer.Start, buffer.Start);
                    }
                    else
                    {
                        connection.Input.Advance(buffer.Start, buffer.End);
                    }
                }
            }
        }

        private static ReadCursor ProcessHeader(HttpContext context, ref ReadableBuffer currentSlice)
        {
            if (!currentSlice.TrySliceTo(HttpConsts.EndOfLine, out ReadableBuffer headerLine, out ReadCursor cursor))
            {
                headerLine = currentSlice;
                currentSlice = currentSlice.Slice(currentSlice.Length);
            }
            else
            {
                currentSlice = currentSlice.Slice(cursor).Slice(HttpConsts.EndOfLine.Length);
            }
            if (!headerLine.TrySliceTo(HttpConsts.HeaderSplit, out ReadableBuffer key, out cursor))
            {
                throw new NotImplementedException();
            }
            var keyString = key.GetUtf8String();
            var values = headerLine.Slice(cursor).Slice(HttpConsts.HeaderSplit.Length).GetUtf8String().Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            if (keyString == HttpConsts.ContentLengthHeader)
            {
                context.Response.ContentLength = int.Parse(values[0]);
            }
            context.Response.Headers[keyString] = new Microsoft.Extensions.Primitives.StringValues(values);
            return cursor;
        }

        public static Task<bool> ReceiveBodyAsync(this IPipeConnection connection, HttpContext context, ILogger logger)
        {
            if (context.Response.Headers[HttpConsts.TransferEncodingHeader] == HttpConsts.ChunkedContentType)
            {
                return connection.ReceiveChunkedBody(context, logger);
            }
            if (context.Response.ContentLength > 0)
            {
                return connection.ReceiveContentLengthBody(context);
            }
            return _cachedTask;
        }

        private static async Task<bool> ReceiveContentLengthBody(this IPipeConnection connection, HttpContext context)
        {
            var contentSize = (int)context.Response.ContentLength.Value;
            while (true)
            {
                var reader = await connection.Input.ReadAsync();
                var buffer = reader.Buffer;
                try
                {
                    var amountTowrite = Math.Min(contentSize, buffer.Length);
                    await buffer.Slice(0, amountTowrite).CopyToAsync(context.Response.Body);
                    contentSize -= amountTowrite;
                    buffer = buffer.Slice(amountTowrite);
                    if (contentSize == 0)
                    {
                        return !reader.IsCompleted;
                    }
                }
                finally
                {
                    connection.Input.Advance(buffer.Start, buffer.End);
                }
            }
        }

        private static async Task<bool> ReceiveChunkedBody(this IPipeConnection connection, HttpContext context, ILogger logger)
        {
            var nextChunkSize = 0;
            var lastChunk = false;
            while (true)
            {
                var reader = await connection.Input.ReadAsync();
                var buffer = reader.Buffer;
                try
                {
                    while (true)
                    {
                        if (nextChunkSize == 0)
                        {
                            //looking for a length, slice a line
                            if (!buffer.TrySliceTo(HttpConsts.EndOfLine, out ReadableBuffer lengthLine, out ReadCursor cursor))
                            {
                                break;
                            }
                            //We have the first line
                            await buffer.Slice(0, lengthLine.Length + HttpConsts.EndOfLine.Length).CopyToAsync(context.Response.Body);
                            buffer = buffer.Slice(cursor).Slice(HttpConsts.EndOfLine.Length);
                            nextChunkSize = int.Parse(lengthLine.GetAsciiString(), System.Globalization.NumberStyles.HexNumber);
                            if (nextChunkSize == 0)
                            {
                                lastChunk = true;
                            }
                            nextChunkSize += 2;
                        }
                        if (buffer.Length > 0)
                        {
                            var dataToSend = Math.Min(nextChunkSize, buffer.Length);
                            await buffer.Slice(0, dataToSend).CopyToAsync(context.Response.Body);
                            buffer = buffer.Slice(dataToSend);
                            nextChunkSize -= dataToSend;
                            if (nextChunkSize == 0 && lastChunk)
                            {
                                logger?.LogInformation("Finished sending chunked data remaining buffer size {bufferSize}", buffer.Length);
                                return !reader.IsCompleted;
                            }
                            if (buffer.Length == 0)
                            {
                                break;
                            }
                        }
                    }
                }
                finally
                {
                    connection.Input.Advance(buffer.Start, buffer.End);
                }
            }
        }
    }
}
