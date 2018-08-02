using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Core.Adapter.Internal;

namespace CondenserDotNet.Middleware.WindowsAuthentication
{
    public class AuthenticationConnectionFilter : IConnectionAdapter
    {
        public bool IsHttps => false;

        public Task<IAdaptedConnection> OnConnectionAsync(ConnectionAdapterContext context)
        {
            var authFeature = new WindowsAuthFeature();
            context.Features.Set<IWindowsAuthFeature>(authFeature);
            var adapted = new AuthenticationAdaptedConnection(context.ConnectionStream, authFeature);
            return Task.FromResult<IAdaptedConnection>(adapted);
        }

        public class AuthenticationAdaptedConnection : IAdaptedConnection
        {
            private readonly Stream _connectionStream;
            private WindowsAuthFeature _windowsAuth;

            public AuthenticationAdaptedConnection(Stream stream, WindowsAuthFeature authFeature)
            {
                _connectionStream = stream;
                _windowsAuth = authFeature;
            }

            public Stream ConnectionStream => _connectionStream;

            public void Dispose() => _windowsAuth.Dispose();
        }
    }
}
