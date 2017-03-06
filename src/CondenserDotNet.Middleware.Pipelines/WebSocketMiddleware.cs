using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.IO.Pipelines.Text.Primitives;
using System.Text;
using System.Text.Formatting;
using System.Threading.Tasks;
using CondenserDotNet.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Middleware.Pipelines
{
    public class WebsocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly PipeFactory _factory;
        private static readonly byte[] _space = Encoding.UTF8.GetBytes(" ");
        private static readonly byte[] _endOfLine = Encoding.UTF8.GetBytes("\r\n");
        private static readonly byte[] _headersEnd = Encoding.UTF8.GetBytes("\r\n\r\n");
        private static readonly byte[] _headerSplit = Encoding.UTF8.GetBytes(": ");

        public WebsocketMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, PipeFactory factory)
        {
            _next = next;
            _factory = factory;
            _logger = loggerFactory?.CreateLogger<WebsocketMiddleware>();
        }

        public Task Invoke(HttpContext context)
        {
            var upgradeFeature = context.Features.Get<IHttpUpgradeFeature>();
            if (upgradeFeature != null && context.Request.Headers["Upgrade"].Count > 0)
            {
                return DoWebSocket(context, upgradeFeature);
            }
            return _next.Invoke(context);
        }

        private async Task DoWebSocket(HttpContext context, IHttpUpgradeFeature upgrade)
        {
            var service = context.Features.Get<IService>();
            var endPoint = service.IpEndPoint;
            var socket = await System.IO.Pipelines.Networking.Sockets.SocketConnection.ConnectAsync(endPoint, _factory);
            try
            {
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
                    writer.Append(string.Join(", ", (IEnumerable<string>)header.Value), TextEncoder.Utf8);
                    writer.Write(_endOfLine);
                }
                writer.Write(_endOfLine);
                await writer.FlushAsync();
                while (true)
                {
                    var reader = await socket.Input.ReadAsync();
                    var buffer = reader.Buffer;
                    try
                    {
                        if (!buffer.TrySliceTo(_headersEnd, out ReadableBuffer headers, out ReadCursor cursor))
                        {
                            continue;
                        }
                        buffer = buffer.Slice(cursor).Slice(_headersEnd.Length);
                        if (!headers.TrySliceTo(_endOfLine, out ReadableBuffer line, out cursor))
                        {
                            throw new InvalidOperationException();
                        }
                        headers = headers.Slice(cursor).Slice(_endOfLine.Length);
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
                            var values = headerLine.Slice(cursor).Slice(_headerSplit.Length).GetUtf8String().Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                            context.Response.Headers[key.GetUtf8String()] = new Microsoft.Extensions.Primitives.StringValues(values);
                        }
                        break;
                    }
                    finally
                    {
                        socket.Input.Advance(buffer.Start, buffer.End);
                    }
                }
                var upgradedStream = await upgrade.UpgradeAsync();
                await Task.WhenAll(upgradedStream.CopyToEndAsync(socket.Output), socket.Input.CopyToEndAsync(upgradedStream));
                upgradedStream.Dispose();
            }
            finally
            {
                await socket.DisposeAsync();
            }
        }
    }
}
