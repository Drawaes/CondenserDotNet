using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using static Interop.Secur32;

namespace CondenserDotNet.Middleware.WindowsAuthentication
{
    public class WindowsAuthFeature : IDisposable
    {
        private static SecurityHandle _credentialsHandle;
        private WindowsHandshake _handshake;

        static WindowsAuthFeature()
        {
            var result = AcquireCredentialsHandle(null,"Negotiate", CredentialsUse.SECPKG_CRED_INBOUND,
                            IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero, out _credentialsHandle, out SecurityInteger timeSpan);
            if (result != SEC_RESULT.SEC_E_OK)
            {
                throw new InvalidOperationException();
            }
        }

        public WindowsIdentity Identity { get; set; }

        public void Dispose()
        {
            Identity?.Dispose();
            _handshake?.Dispose();
            Identity = null;
        }

        public string ProcessHandshake(byte[] token)
        {
            if(_handshake == null)
            {
                _handshake = new WindowsHandshake(_credentialsHandle);
            }
            return _handshake.AcceptSecurityToken(token);
        }

        public WindowsIdentity GetUser()
        {
            var user = _handshake.User;
            _handshake.Dispose();
            _handshake = null;
            return user;
        }
    }
}
