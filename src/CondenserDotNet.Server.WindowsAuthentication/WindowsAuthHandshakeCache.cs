using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using static Interop.Secur32;

namespace CondenserDotNet.Server.WindowsAuthentication
{
    public class WindowsAuthHandshakeCache
    {
        private SecurityHandle _credentialsHandle;
        private ConcurrentDictionary<string, WindowsHandshake> _inflightHandshakes = new ConcurrentDictionary<string, WindowsHandshake>();
        private TimeSpan _checkForExpiry = new TimeSpan(0, 0, 1, 0);
        private TimeSpan _maxHandshakeAge = new TimeSpan(0, 0, 1, 0);
        private DateTime _lastCheck = DateTime.UtcNow;

        public WindowsAuthHandshakeCache()
        {
            SecurityInteger timeSpan;
            var result = AcquireCredentialsHandle(
                    null,
                    "Negotiate",
                    CredentialsUse.SECPKG_CRED_INBOUND,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    0,
                    IntPtr.Zero,
                    out _credentialsHandle,
                    out timeSpan);
            if (result != SEC_RESULT.SEC_E_OK)
            {
                throw new InvalidOperationException();
            }
        }

        public string ProcessHandshake(byte[] token, string sessionId)
        {
            var handshakeState = _inflightHandshakes.GetOrAdd(sessionId, (key) => new WindowsHandshake(key, _credentialsHandle));
            if (_lastCheck < (DateTime.UtcNow - _checkForExpiry))
            {
                _lastCheck = DateTime.UtcNow;
                Task.Run(() => CleanUp());
            }
            return handshakeState.AcceptSecurityToken(token);
        }

        private void CleanUp()
        {
            foreach (var item in _inflightHandshakes.Values)
            {
                if (item.DateStarted < (DateTime.UtcNow - _maxHandshakeAge))
                {
                    WindowsHandshake handshake;
                    if (_inflightHandshakes.TryRemove(item.SessionId, out handshake))
                    {
                        handshake.Dispose();
                    }
                }
            }
        }

        public WindowsIdentity GetUser(string session)
        {
            WindowsHandshake shake;
            if (_inflightHandshakes.TryGetValue(session, out shake))
            {
                return shake.User;
            }
            return null;
        }

        public void RemoveUser(string session)
        {
            WindowsHandshake handshake;
            _inflightHandshakes.TryRemove(session, out handshake);
        }
    }
}
