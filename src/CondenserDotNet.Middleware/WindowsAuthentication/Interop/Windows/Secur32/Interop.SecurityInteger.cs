using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
    internal partial class Secur32
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct SecurityInteger
        {
            public uint LowPart;
            public int HighPart;
        }
    }
}
