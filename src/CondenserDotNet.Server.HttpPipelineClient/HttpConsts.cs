using System;
using System.Collections.Generic;
using System.Text;

namespace CondenserDotNet.Server.HttpPipelineClient
{
    public static class HttpConsts
    {
        public static readonly byte[] Space = Encoding.UTF8.GetBytes(" ");
        public static readonly byte[] EndOfLine = Encoding.UTF8.GetBytes("\r\n");
        public static readonly byte[] HeadersEnd = Encoding.UTF8.GetBytes("\r\n\r\n");
        public static readonly byte[] HeaderSplit = Encoding.UTF8.GetBytes(": ");
        public static readonly byte[] ConnectionHeader = Encoding.UTF8.GetBytes("Connection: keep-alive\r\n\r\n");
    }
}
