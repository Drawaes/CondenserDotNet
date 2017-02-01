using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
    internal partial class Secur32
    {
        [Flags]
        internal enum CredentialsUse : int
        {
            SECPKG_CRED_INBOUND = 1,
            SECPKG_CRED_OUTBOUND = 2,
            SECPKG_CRED_BOTH = (SECPKG_CRED_OUTBOUND | SECPKG_CRED_INBOUND),
        }
    }
}
