using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Core.Adapter.Internal;
using Microsoft.AspNetCore.Server.Kestrel;
using System.IO;
using Microsoft.AspNetCore.Http.Features;

namespace CondenserDotNet.Middleware.ProtocolSwitcher
{
    public class ProtocolSwitchConnectionFilter : IConnectionAdapter
    {
        private bool _isHttp;

        public bool IsHttps => _isHttp;

        public async Task<IAdaptedConnection> OnConnectionAsync(ConnectionAdapterContext context)
        {
            var connection = context.ConnectionStream;
            var back2Back = new BackToBackStream(connection);
            
            var firstByte = new byte[1];
            var bytesRead = await connection.ReadAsync(firstByte, 0, 1);
            back2Back.FirstByte = firstByte[0];
            if (firstByte[0] == 0x16)
            {
                context.Features.Set<ITlsConnectionFeature>(new TlsConnectionFeature());
                _isHttp = true;
            }

            
            throw new NotImplementedException();
            //await _previous.OnConnectionAsync(context);
            //var previousRequest = context.PrepareRequest;
            //context.PrepareRequest = features =>
            //{
            //    previousRequest?.Invoke(features);
            //};
        }

        public class ProtocolSwitcherAdaptedConnection : IAdaptedConnection
        {
            private readonly Stream _adaptedConnection;

            public Stream ConnectionStream => _adaptedConnection;

            public ProtocolSwitcherAdaptedConnection(Stream adaptedConnection) => _adaptedConnection = adaptedConnection;

            public void Dispose() => throw new NotImplementedException();
        }
    }
}
