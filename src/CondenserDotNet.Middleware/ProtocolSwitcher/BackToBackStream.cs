using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CondenserDotNet.Middleware.ProtocolSwitcher
{
    public class BackToBackStream : Stream
    {
        private readonly byte _firstByte;
        private bool _usedFirstByte;
        private readonly Stream _innerStream;

        public BackToBackStream(byte firstByte, Stream innerStream)
        {
            _innerStream = innerStream;
            _firstByte = firstByte;
        }

        public override bool CanRead => _innerStream.CanRead;
        public override bool CanSeek => _innerStream.CanSeek;
        public override bool CanWrite => _innerStream.CanWrite;
        public override long Length => _innerStream.Length;
        public override long Position { get => _innerStream.Position; set => _innerStream.Position = value; }

        public override void Flush() => _innerStream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            int returnCount = 0;
            if (!_usedFirstByte)
            {
                buffer[offset] = _firstByte;
                offset++;
                count--;
                returnCount++;
                _usedFirstByte = true;
            }
            return _innerStream.Read(buffer, offset, count) + returnCount;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();
        public override void SetLength(long value) => throw new NotImplementedException();
        public override void Write(byte[] buffer, int offset, int count) => _innerStream.Write(buffer, offset, count);
        public override Task FlushAsync(CancellationToken cancellationToken) => _innerStream.FlushAsync(cancellationToken);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (!_usedFirstByte)
            {
                buffer[offset] = _firstByte;
                offset++;
                _usedFirstByte = true;
                count--;
                return _innerStream.ReadAsync(buffer, offset, count, cancellationToken).ContinueWith((result) => result.Result + 1);
            }
            return _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            _innerStream.Dispose();
        }
    }
}
