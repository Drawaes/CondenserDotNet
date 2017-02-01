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
            uint cbBuffer;
            SecurityBufferType BufferType;
            IntPtr pvBuffer;
        }

        [Flags]
        internal enum NtlmFlags : int
        {
            // The client sets this flag to indicate that it supports Unicode strings.
            NegotiateUnicode = 0x00000001,
            // This is set to indicate that the client supports OEM strings.
            NegotiateOem = 0x00000002,
            // This requests that the server send the authentication target with the Type 2 reply.
            RequestTarget = 0x00000004,
            // Indicates that NTLM authentication is supported.
            NegotiateNtlm = 0x00000200,
            // When set, the client will send with the message the name of the domain in which the workstation has membership.
            NegotiateDomainSupplied = 0x00001000,
            // Indicates that the client is sending its workstation name with the message.  
            NegotiateWorkstationSupplied = 0x00002000,
            // Indicates that communication between the client and server after authentication should carry a "dummy" signature.
            NegotiateAlwaysSign = 0x00008000,
            // Indicates that this client supports the NTLM2 signing and sealing scheme; if negotiated, this can also affect the response calculations.
            NegotiateNtlm2Key = 0x00080000,
            // Indicates that this client supports strong (128-bit) encryption.
            Negotiate128 = 0x20000000,
            // Indicates that this client supports medium (56-bit) encryption.
            Negotiate56 = (unchecked((int)0x80000000))
        }
    }
}
