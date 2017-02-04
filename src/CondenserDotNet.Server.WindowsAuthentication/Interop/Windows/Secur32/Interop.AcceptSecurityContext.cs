using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
    internal partial class Secur32
    {
        [DllImport(Libraries.Secur32, CharSet = CharSet.Unicode, SetLastError = false)]
        internal static extern SEC_RESULT AcceptSecurityContext(SecurityHandle phCredential, ref SecurityHandle phContext,
        ref SecBufferDesc pInput, ASC_REQ fContextReq, Data_Rep TargetDataRep, out SecurityHandle phNewContext,
            ref SecBufferDesc pOutput, out uint pfContextAttr, out SecurityInteger ptsTimeStamp);
        [DllImport(Libraries.Secur32, CharSet = CharSet.Unicode, SetLastError = false)]
        internal static extern SEC_RESULT AcceptSecurityContext(SecurityHandle phCredential, IntPtr phContext,
        ref SecBufferDesc pInput, ASC_REQ fContextReq, Data_Rep TargetDataRep, out SecurityHandle phNewContext,
            ref SecBufferDesc pOutput, out uint pfContextAttr, out SecurityInteger ptsTimeStamp);
    }
}
