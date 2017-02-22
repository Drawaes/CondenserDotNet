using System;
using System.IO.Pipelines;
using System.IO.Pipelines.Networking.Sockets;
using System.IO.Pipelines.Text.Primitives;
using System.Linq;
using System.Text;
using System.Text.Formatting;
using System.Threading.Tasks;
using CondenserDotNet.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Server.HttpPipelineClient
{
    public class ServiceCallMiddleware
    {
        private RequestDelegate _next;
        private ILogger _logger;
        private static readonly byte[] _space = Encoding.UTF8.GetBytes(" ");
        private static readonly byte[] _endOfLine = Encoding.UTF8.GetBytes("\r\n");
        private static readonly byte[] _headersEnd = Encoding.UTF8.GetBytes("\r\n\r\n");
        private static readonly byte[] _headerSplit = Encoding.UTF8.GetBytes(": ");
        private SocketConnection _socket;

        public ServiceCallMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory?.CreateLogger<ServiceCallMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            var service = context.Features.Get<IService>();
            var endPoint = service.IpEndPoint;
            var socket = await System.IO.Pipelines.Networking.Sockets.SocketConnection.ConnectAsync(endPoint);
            var writer = socket.Output.Alloc();
            writer.Append(context.Request.Method, TextEncoder.Utf8);
            writer.Write(_space);
            writer.Append(context.Request.Path.Value, TextEncoder.Utf8);
            writer.Write(_space);
            writer.Append(context.Request.Protocol, TextEncoder.Utf8);
            writer.Write(_endOfLine);
            foreach (var header in context.Request.Headers)
            {
                writer.Append(header.Key, TextEncoder.Utf8);
                writer.Write(_headerSplit);
                writer.Append(string.Join(", ", header.Value), TextEncoder.Utf8);
                writer.Write(_endOfLine);
            }
            writer.Write(_endOfLine);
            await writer.FlushAsync();
            await ReadResponse(context);
        }

        private enum ParseMode
        {
            InitialLine,
            Headers,
            Body
        }

        private async Task ReadResponse(HttpContext context)
        {
            var mode = ParseMode.InitialLine;
            bool isChunked = false;
            int contentLength = 0;
            while (true)
            {
                var reader = await _socket.Input.ReadAsync();
                var buffer = reader.Buffer;
                try
                {
                    if(mode == ParseMode.InitialLine)
                    {
                        if(!buffer.TrySliceTo(_endOfLine, out ReadableBuffer currentSlice, out ReadCursor cursor))
                        {
                            continue;
                        }
                        //Have the first line, don't care about the rest right now
                        buffer = buffer.Slice(cursor).Slice(_endOfLine.Length);
                        //ignore the protocol stuff
                        context.Response.StatusCode = int.Parse(currentSlice.Split(_space[0]).Skip(1).First().GetAsciiString());
                        mode = ParseMode.Headers;
                    }
                    if (mode == ParseMode.Headers)
                    {
                        if (!buffer.TrySliceTo(_headersEnd, out ReadableBuffer headers, out ReadCursor cursor))
                        {
                            continue;
                        }
                        //dont care about the rest
                        buffer = buffer.Slice(cursor).Slice(_headersEnd.Length);
                        mode = ParseMode.Body;
                        while (headers.Length > 0)
                        {
                            if (!headers.TrySliceTo(_endOfLine, out ReadableBuffer headerLine, out cursor))
                            {
                                headerLine = headers;
                                headers = headers.Slice(headers.Length);
                            }
                            else
                            {
                                headers = headers.Slice(cursor).Slice(_endOfLine.Length);
                            }
                            if (!headerLine.TrySliceTo(_headerSplit, out ReadableBuffer key, out cursor))
                            {
                                throw new NotImplementedException();
                            }
                            var keyString = key.GetUtf8String();
                            var values = headerLine.Slice(cursor).Slice(_headerSplit.Length).GetUtf8String().Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                            if (keyString == "Transfer-Encoding")
                            {
                                if (values[0] == "chunked")
                                {
                                    isChunked = true;
                                }
                            }
                            context.Response.Headers[key.GetUtf8String()] = new Microsoft.Extensions.Primitives.StringValues(values);
                        }
                        if(mode == ParseMode.Body)
                        {
                            if(isChunked)
                            {
                                
                            }
                            else
                            {
                                if(context.Response.ContentLength == 0)
                                {
                                    //Finished
                                    return;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    _socket.Input.Advance(buffer.Start, buffer.End);
                }
            }
        }
    }
}
