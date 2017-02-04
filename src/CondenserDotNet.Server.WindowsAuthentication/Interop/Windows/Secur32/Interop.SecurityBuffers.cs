using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
    internal partial class Secur32
    {
        internal enum SecurityBufferType : uint
        {
            SECBUFFER_VERSION = 0,
            SECBUFFER_EMPTY = 0,
            SECBUFFER_DATA = 1,
            SECBUFFER_TOKEN = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SecBufferDesc
        {
            public uint ulVersion;
            public uint cBuffers;
            public IntPtr pBuffers;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SecBuffer
        {
            public uint cbBuffer;
            public SecurityBufferType BufferType;
            public IntPtr pvBuffer;
        }
    }
}
