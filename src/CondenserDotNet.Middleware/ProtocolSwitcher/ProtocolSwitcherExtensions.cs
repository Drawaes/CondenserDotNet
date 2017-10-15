using Microsoft.AspNetCore.Server.Kestrel;

namespace CondenserDotNet.Middleware.ProtocolSwitcher
{
    public static class ProtocolSwitcherExtensions
    {
        /*public static KestrelServerOptions Switcheroo(this KestrelServerOptions options)
        {
            var prevFilter = options.ConnectionFilter ?? new NoOpConnectionFilter();
            options.ConnectionFilter = new ProtocolSwitchConnectionFilter(prevFilter);
            return options;
        }*/
    }
}
