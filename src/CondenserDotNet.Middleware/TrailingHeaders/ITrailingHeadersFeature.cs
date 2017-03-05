using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace CondenserDotNet.Middleware.TrailingHeaders
{
    public interface ITrailingHeadersFeature
    {
        void RegisterHeader(string name, Func<string> contentCallback);
    }
}
