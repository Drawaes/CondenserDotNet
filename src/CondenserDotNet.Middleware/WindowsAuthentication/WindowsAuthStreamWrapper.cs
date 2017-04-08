using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CondenserDotNet.Middleware.WindowsAuthentication
{
    public class WindowsAuthStreamWrapper : Stream
    {
        private readonly Stream _innerStream;
        private WindowsAuthFeature _authFeature;

        public WindowsAuthStreamWrapper(Stream inStream, WindowsAuthFeature authFeature)
        {
            _authFeature = authFeature ?? throw new ArgumentNullException(nameof(authFeature));
            _innerStream = inStream ?? throw new ArgumentNullException(nameof(inStream));
        }

        public override bool CanRead => _innerStream.CanRead;
        public override bool CanSeek => _innerStream.CanSeek;
        public override bool CanWrite => _innerStream.CanWrite;
        public override long Length => _innerStream.Length;
        public override long Position { get => _innerStream.Position; set => _innerStream.Position = value; }
        public IWindowsAuthFeature AuthFeature => _authFeature;

        public override void Flush() => _innerStream.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _innerStream.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => _innerStream.Seek(offset, origin);
        public override void SetLength(long value) => _innerStream.SetLength(value);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            _innerStream.ReadAsync(buffer, offset, count, cancellationToken);

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) =>
            _innerStream.CopyToAsync(destination, bufferSize, cancellationToken);

        public override Task FlushAsync(CancellationToken cancellationToken) =>
            _innerStream.FlushAsync(cancellationToken);

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            _innerStream.WriteAsync(buffer, offset, count, cancellationToken);

        public override void Write(byte[] buffer, int offset, int count) => _innerStream.Write(buffer, offset, count);

        protected override void Dispose(bool disposing)
        {
            _authFeature.Dispose();
            base.Dispose(disposing);
        }
    }
}
