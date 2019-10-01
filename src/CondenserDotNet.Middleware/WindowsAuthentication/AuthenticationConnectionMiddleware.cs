using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;

namespace CondenserDotNet.Middleware.WindowsAuthentication
{
    public class AuthenticationConnectionFilter : ConnectionHandler
    {
        private ConnectionDelegate _next;

        public AuthenticationConnectionFilter(ConnectionDelegate next) => _next = next;

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            var authFeature = new WindowsAuthFeature();
            connection.Features.Set<IWindowsAuthFeature>(authFeature);

            try
            {
                await _next(connection);
            }
            finally
            {
                authFeature.Dispose();
            }
        }
    }
}
