using System.Runtime.InteropServices;

internal partial class Interop
{
    internal partial class Secur32
    {
        [DllImport(Libraries.Secur32, CharSet = CharSet.Unicode, SetLastError = false)]
        internal static extern SEC_RESULT DeleteSecurityContext(SecurityHandle phCredential);
    }
}
