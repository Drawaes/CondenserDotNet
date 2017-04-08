using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CondenserDotNet.Middleware.ProtocolSwitcher
{
    public class BackToBackStream : Stream
    {
        private bool _usedFirstByte;
        private readonly Stream _innerStream;

        public BackToBackStream(Stream innerStream) => _innerStream = innerStream;

        public byte FirstByte { get; set; }
        public override bool CanRead => _innerStream.CanRead;
        public override bool CanSeek => _innerStream.CanSeek;
        public override bool CanWrite => _innerStream.CanWrite;
        public override long Length => _innerStream.Length;
        public override long Position { get => _innerStream.Position; set => _innerStream.Position = value; }

        public override void Flush() => _innerStream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            var returnCount = 0;
            if (!_usedFirstByte)
            {
                buffer[offset] = FirstByte;
                offset++;
                count--;
                returnCount++;
                _usedFirstByte = true;
            }
            returnCount += _innerStream.Read(buffer, offset, count);
            return returnCount;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();
        public override void SetLength(long value) => throw new NotImplementedException();
        public override void Write(byte[] buffer, int offset, int count) => _innerStream.Write(buffer, offset, count);
        public override Task FlushAsync(CancellationToken cancellationToken) => _innerStream.FlushAsync(cancellationToken);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (!_usedFirstByte)
            {
                buffer[offset] = FirstByte;
                offset++;
                _usedFirstByte = true;
                count--;
                return _innerStream.ReadAsync(buffer, offset, count, cancellationToken).ContinueWith((result) => result.Result + 1);
            }
            return _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            _innerStream.WriteAsync(buffer, offset, count, cancellationToken);

        public override void WriteByte(byte value) => _innerStream.WriteByte(value);
        protected override void Dispose(bool disposing) => _innerStream.Dispose();
    }
}
