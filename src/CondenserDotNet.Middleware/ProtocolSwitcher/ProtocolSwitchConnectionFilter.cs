using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Filter;
using Microsoft.AspNetCore.Server.Kestrel;

namespace CondenserDotNet.Middleware.ProtocolSwitcher
{
    public class ProtocolSwitchConnectionFilter:IConnectionFilter
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
            await _previous.OnConnectionAsync(context);

            var previousRequest = context.PrepareRequest;
            var firstByte = new byte[1];

            await context.Connection.ReadAsync(firstByte, 0, 1);
            
            if (firstByte[0] == 0x16)
            {
                context.Address = ServerAddress.FromUrl($"https://{context.Address.Host}:{context.Address.Port}");
            }
            var connection = context.Connection;
            var back2Back = new BackToBackStream(firstByte[0], connection);
            context.Connection = back2Back;
            
            context.PrepareRequest = features =>
            {
                previousRequest?.Invoke(features);
            };
        }
    }
}
