using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using static Interop.Secur32;

namespace CondenserDotNet.Server.Authentication
{
    public class NtlmHandshake
    {
        private SecurityHandle _context;
        private string _sessionId;
        private SecurityHandle _ntlmHandle;
        private WindowsIdentity _identity;
        private static readonly ASC_REQ _requestType = ASC_REQ.ASC_REQ_CONFIDENTIALITY | ASC_REQ.ASC_REQ_REPLAY_DETECT
            | ASC_REQ.ASC_REQ_SEQUENCE_DETECT | ASC_REQ.ASC_REQ_CONNECTION;

        internal NtlmHandshake(string sessionId, SecurityHandle ntlmHandle)
        {
            _sessionId = sessionId;
            _ntlmHandle = ntlmHandle;
        }

        public unsafe string AcceptSecurityToken(Span<byte> token)
        {
            var bufferPtr = stackalloc byte[token.Length];
            var bufferSpan = new Span<byte>(bufferPtr, token.Length);
            token.CopyTo(bufferSpan);
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
            if(_context.HighPart == _context.LowPart && _context.HighPart == IntPtr.Zero)
            {
                result = AcceptSecurityContext(_ntlmHandle, IntPtr.Zero, ref bufferDesc, _requestType, Data_Rep.SECURITY_NATIVE_DREP,
                out _context, ref outBufferDesc, out contextAttribute, out timeStamp);
            }
            else
            {
                result = AcceptSecurityContext(_ntlmHandle, ref _context, ref bufferDesc, _requestType, Data_Rep.SECURITY_NATIVE_DREP,
                out _context, ref outBufferDesc, out contextAttribute, out timeStamp);
            }
            
            if (result == SEC_RESULT.SEC_I_CONTINUE_NEEDED)
            {
                var returnToken = "NTLM " + Convert.ToBase64String((new Span<byte>(outBufferPtr, (int)outBuffer[0].cbBuffer).ToArray()));
                return returnToken;
            }

            if (result == SEC_RESULT.SEC_E_OK)
            {
                IntPtr handle;
                result = QuerySecurityContextToken(ref _context, out handle);
                _identity = new WindowsIdentity(handle);
                //CloseHandle(handle);
                return null;
            }

            throw new NotImplementedException();
        }
    }
}
