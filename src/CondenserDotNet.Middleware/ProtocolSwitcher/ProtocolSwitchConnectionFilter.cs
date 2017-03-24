using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Filter;
using Microsoft.AspNetCore.Server.Kestrel;

namespace CondenserDotNet.Middleware.ProtocolSwitcher
{
    public class ProtocolSwitchConnectionFilter : IConnectionFilter
    {
        private readonly IConnectionFilter _previous;

        public ProtocolSwitchConnectionFilter(IConnectionFilter previous)
        {
            _previous = previous;
            if (_previous == null)
            {
                throw new ArgumentNullException();
            }
        }

        public async Task OnConnectionAsync(ConnectionFilterContext context)
        {
            var connection = context.Connection;
            var back2Back = new BackToBackStream(connection);
            context.Connection = back2Back;
            var firstByte = new byte[1];
            var bytesRead = await connection.ReadAsync(firstByte, 0, 1);
            back2Back.FirstByte = firstByte[0];
            if (firstByte[0] == 0x16)
            {
                context.Address = ServerAddress.FromUrl($"https://{context.Address.Host}:{context.Address.Port}");
            }

            await _previous.OnConnectionAsync(context);
            var previousRequest = context.PrepareRequest;
            context.PrepareRequest = features =>
            {
                previousRequest?.Invoke(features);
            };
        }
    }
}
