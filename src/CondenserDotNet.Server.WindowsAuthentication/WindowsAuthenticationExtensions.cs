using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.AspNetCore.Server.Kestrel.Filter;

namespace CondenserDotNet.Server.WindowsAuthentication
{
    public static class WindowsAuthenticationExtensions
    {
        public static KestrelServerOptions UseWindowsAuthentication(this KestrelServerOptions options)
        {
            var prevFilter = options.ConnectionFilter ?? new NoOpConnectionFilter();
            options.ConnectionFilter = new AuthenticationConnectionFilter(prevFilter);
            return options;
        }
    }
}
