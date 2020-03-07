using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;

namespace CondenserDotNet.Middleware.WindowsAuthentication
{
    public class AuthenticationConnectionMiddleware : ConnectionHandler
    {
        private ConnectionDelegate _next;

        public AuthenticationConnectionMiddleware(ConnectionDelegate next) => _next = next;

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            using var authFeature = new WindowsAuthFeature();
            connection.Features.Set<IWindowsAuthFeature>(authFeature);
            await _next(connection).ConfigureAwait(false);
        }
    }
}
