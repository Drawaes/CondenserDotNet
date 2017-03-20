using System.Text;

namespace CondenserDotNet.Middleware.Pipelines
{
    public static class HttpConsts
    {
        public static readonly byte[] Space = Encoding.UTF8.GetBytes(" ");
        public static readonly byte[] EndOfLine = Encoding.UTF8.GetBytes("\r\n");
        public static readonly byte[] HeadersEnd = Encoding.UTF8.GetBytes("\r\n\r\n");
        public static readonly byte[] HeaderSplit = Encoding.UTF8.GetBytes(": ");
        public static readonly byte[] ConnectionHeaderBytes = Encoding.UTF8.GetBytes("Connection: keep-alive\r\n\r\n");
        public static readonly string ContentLengthHeader = "Content-Length";
        public static readonly string ChunkedContentType = "chunked";
        public static readonly string TransferEncodingHeader = "Transfer-Encoding";
    }
}
