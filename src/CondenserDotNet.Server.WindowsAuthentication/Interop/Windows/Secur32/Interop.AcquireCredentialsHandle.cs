using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
    internal partial class Secur32
    {
        [DllImport(Libraries.Secur32, CharSet = CharSet.Unicode, SetLastError = false)]
        public static extern SEC_RESULT AcquireCredentialsHandle(string pszPrincipal, string pszPackage, CredentialsUse fCredentialUse,
            IntPtr pvLogonID, IntPtr pAuthData, int pGetKeyFn, IntPtr pvGetKeyArgument, out SecurityHandle phCredential,
            out SecurityInteger ptsExpiry);
    }
}
