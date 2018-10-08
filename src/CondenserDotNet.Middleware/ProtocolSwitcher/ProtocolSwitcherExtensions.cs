using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace CondenserDotNet.Middleware.ProtocolSwitcher
{
    public static class ProtocolSwitcherExtensions
    {
        public static ListenOptions Switcheroo(this ListenOptions options)
        {
            options.ConnectionAdapters.Add(new ProtocolSwitchConnectionFilter());
            return options;
        }
    }
}
