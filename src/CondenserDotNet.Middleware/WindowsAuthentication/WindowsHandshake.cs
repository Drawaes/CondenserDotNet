using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using static Interop.Secur32;
using static Interop.Kernel32;

namespace CondenserDotNet.Middleware.WindowsAuthentication
{
    public sealed class WindowsHandshake : IDisposable
    {
        private SecurityHandle _context;
        private SecurityHandle _ntlmHandle;
        private WindowsIdentity _identity;
        private readonly DateTime _dateStarted = DateTime.UtcNow;
        private static readonly ASC_REQ _requestType = ASC_REQ.ASC_REQ_CONFIDENTIALITY | ASC_REQ.ASC_REQ_REPLAY_DETECT
            | ASC_REQ.ASC_REQ_SEQUENCE_DETECT | ASC_REQ.ASC_REQ_CONNECTION;

        internal WindowsHandshake(SecurityHandle ntlmHandle) => _ntlmHandle = ntlmHandle;

        public WindowsIdentity User => _identity;
        public DateTime DateStarted => _dateStarted;

        public unsafe string AcceptSecurityToken(string returnTokenType, byte[] token)
        {
            fixed (byte* bufferPtr = token)
            {
                var buffer = stackalloc SecBuffer[1];
                buffer[0].BufferType = SecurityBufferType.SECBUFFER_TOKEN;
                buffer[0].cbBuffer = (uint)token.Length;
                buffer[0].pvBuffer = (IntPtr)bufferPtr;

                var outBufferPtr = stackalloc byte[512];
                var outBuffer = stackalloc SecBuffer[1];
                outBuffer[0].BufferType = SecurityBufferType.SECBUFFER_TOKEN;
                outBuffer[0].cbBuffer = 512;
                outBuffer[0].pvBuffer = (IntPtr)outBufferPtr;

                var bufferDesc = new SecBufferDesc()
                {
                    cBuffers = 1,
                    pBuffers = (IntPtr)buffer,
                    ulVersion = 0
                };

                var outBufferDesc = new SecBufferDesc()
                {
                    cBuffers = 1,
                    pBuffers = (IntPtr)outBuffer,
                    ulVersion = 0
                };
                uint contextAttribute;
                SecurityInteger timeStamp;
                SEC_RESULT result;
                if (_context.HighPart == _context.LowPart && _context.HighPart == IntPtr.Zero)
                {
                    result = AcceptSecurityContext(_ntlmHandle, IntPtr.Zero, ref bufferDesc, _requestType, Data_Rep.SECURITY_NATIVE_DREP,
                    out _context, ref outBufferDesc, out contextAttribute, out timeStamp);
                }
                else
                {
                    result = AcceptSecurityContext(_ntlmHandle, ref _context, ref bufferDesc, _requestType, Data_Rep.SECURITY_NATIVE_DREP,
                    out _context, ref outBufferDesc, out contextAttribute, out timeStamp);
                }
                string returnToken = null;
                if (result == SEC_RESULT.SEC_I_CONTINUE_NEEDED)
                {
                    var byteSpan = new byte[(int)outBuffer[0].cbBuffer];
                    Marshal.Copy((IntPtr)outBufferPtr, byteSpan, 0, byteSpan.Length);
                    returnToken = returnTokenType + Convert.ToBase64String(byteSpan);
                    return returnToken;
                }

                if (result == SEC_RESULT.SEC_E_OK)
                {
                    if (outBuffer[0].cbBuffer > 0)
                    {
                        var byteSpan = new byte[(int)outBuffer[0].cbBuffer];
                        Marshal.Copy((IntPtr)outBufferPtr, byteSpan, 0, byteSpan.Length);
                        returnToken = "Negotiate " + Convert.ToBase64String(byteSpan);
                    }
                    QuerySecurityContextToken(ref _context, out var handle);
                    _identity = new WindowsIdentity(handle);
                    Interop.Kernel32.CloseHandle(handle);
                    return returnToken;
                }
                throw new InvalidOperationException();
            }
        }

        public void Dispose()
        {
            if (_context.HighPart != IntPtr.Zero || _context.LowPart != IntPtr.Zero)
            {
                FreeCredentialsHandle(_context);
                DeleteSecurityContext(_context);
                _context = default(SecurityHandle);
            }
            GC.SuppressFinalize(this);
        }

        ~WindowsHandshake()
        {
            Dispose();
        }
    }
}
