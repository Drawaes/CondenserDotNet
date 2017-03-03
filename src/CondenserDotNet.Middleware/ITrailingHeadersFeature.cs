using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace CondenserDotNet.Middleware
{
    public interface ITrailingHeadersFeature
    {
        void RegisterHeader(string name, Func<string> contentCallback);
    }
}
