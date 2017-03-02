using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.AspNetCore.Server.Kestrel.Filter;

namespace CondenserDotNet.ProtocolSwitcher
{
    public static class ProtocolSwitcherExtensions
    {
        public static KestrelServerOptions Switcheroo(this KestrelServerOptions options)
        {
            var prevFilter = options.ConnectionFilter ?? new NoOpConnectionFilter();
            options.ConnectionFilter = new ProtocolSwitchConnectionFilter(prevFilter);
            return options;
        }
    }
}
