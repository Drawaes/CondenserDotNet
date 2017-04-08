using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Internal.Http;

namespace CondenserDotNet.Middleware.TrailingHeaders
{
    public class ChunkingStream : Stream
    {
        private Stream _innerStream;
        private static readonly byte[] _endChunkBytes = Encoding.ASCII.GetBytes("\r\n");

        public override bool CanRead => throw new NotImplementedException();
        public override bool CanSeek => throw new NotImplementedException();
        public override bool CanWrite => true;
        public override long Length => throw new NotImplementedException();
        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Stream InnerStream { get => _innerStream; set => _innerStream = value; }
        public override void Flush() => _innerStream.Flush();

        public override Task FlushAsync(CancellationToken cancellationToken) => _innerStream.FlushAsync(cancellationToken);
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
        {
            var beginChunkBytes = ChunkWriter.BeginChunkBytes(count);

            _innerStream.Write(beginChunkBytes.Array, beginChunkBytes.Offset, beginChunkBytes.Count);
            _innerStream.Write(buffer, offset, count);
            _innerStream.Write(_endChunkBytes, 0, _endChunkBytes.Length);
        }

        public async override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var beginChunkBytes = ChunkWriter.BeginChunkBytes(count);

            await _innerStream.WriteAsync(beginChunkBytes.Array, beginChunkBytes.Offset, beginChunkBytes.Count, cancellationToken);
            await _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
            await _innerStream.WriteAsync(_endChunkBytes, 0, _endChunkBytes.Length, cancellationToken);
        }
    }
}
