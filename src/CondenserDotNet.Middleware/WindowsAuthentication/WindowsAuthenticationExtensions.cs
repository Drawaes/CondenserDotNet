using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace CondenserDotNet.Middleware.WindowsAuthentication
{
    public static class WindowsAuthenticationExtensions
    {
        public static ListenOptions UseWindowsAuthentication(this ListenOptions options)
        {
            options.Use(next =>
            {
                var middleware = new AuthenticationConnectionMiddleware(next);
                return middleware.OnConnectedAsync;
            });
            return options;
        }

        public static IApplicationBuilder UseWindowsAuthentication(this IApplicationBuilder self)
        {
            self.UseMiddleware<WindowsAuthenticationMiddleware>();
            return self;
        }
    }
}
