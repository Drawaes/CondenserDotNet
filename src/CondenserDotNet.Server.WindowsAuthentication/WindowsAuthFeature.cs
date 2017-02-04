using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace CondenserDotNet.Server.WindowsAuthentication
{
    public class WindowsAuthFeature : IDisposable
    {
        public WindowsIdentity Identity { get; set; }

        public void Dispose()
        {
            Identity?.Dispose();
            Identity = null;
        }
    }
}
