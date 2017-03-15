using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
    internal partial class Secur32
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct SecurityHandle
        {
            public IntPtr LowPart;
            public IntPtr HighPart;
        }
    }
}
