using System;

namespace CondenserDotNet.Middleware.TrailingHeaders
{
    public interface ITrailingHeadersFeature
    {
        void RegisterHeader(string name, Func<string> contentCallback);
    }
}
