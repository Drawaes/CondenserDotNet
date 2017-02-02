using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Interop.Secur32;

namespace CondenserDotNet.Server.Authentication
{
    public class SessionCache
    {
        private SecurityHandle _ntlmHandle;
        private ConcurrentDictionary<Guid, NtlmHandshake> _inflightHandshakes = new ConcurrentDictionary<Guid, NtlmHandshake>();

        public SessionCache()
        {
            SecurityInteger timeSpan;
            var result = AcquireCredentialsHandle(
                    null,
                    "NTLM",
                    CredentialsUse.SECPKG_CRED_INBOUND,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    0,
                    IntPtr.Zero,
                    out _ntlmHandle,
                    out timeSpan);
            if(result != SEC_RESULT.SEC_E_OK)
            {
                throw new InvalidOperationException();
            }
        }

        public unsafe string ProcessHandshake(Span<byte> token, Guid sessionId)
        {
            NtlmHandshake handshakeState;
            handshakeState = _inflightHandshakes.GetOrAdd(sessionId, id => new NtlmHandshake(id, _ntlmHandle));
            
            return handshakeState.AcceptSecurityToken(token);
        }
    }
}
