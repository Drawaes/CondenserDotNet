using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
    internal partial class Secur32
    {
        [DllImport(Libraries.Secur32, CharSet = CharSet.Unicode, SetLastError = false)]
        internal static extern SEC_RESULT QuerySecurityContextToken(ref SecurityHandle phContext, out IntPtr phToken);
    }
}
