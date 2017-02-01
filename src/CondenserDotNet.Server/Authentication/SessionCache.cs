using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Interop.Secur32;

namespace CondenserDotNet.Server.Authentication
{
    public class SessionCache
    {
        private SecurityHandle _ntlmHandle;

        public SessionCache()
        {
            SecurityInteger timeSpan;
            AcquireCredentialsHandle(
                    null,
                    "NTLM",
                    CredentialsUse.SECPKG_CRED_INBOUND,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    0,
                    IntPtr.Zero,
                    out _ntlmHandle,
                    out timeSpan);
        }

        public void ProcessHandshake(byte[] token, Guid sessionId)
        {

        }
    }
}
