using System;
using System.Collections.Generic;
using System.Text;

namespace CondenserDotNet.Core.Consul
{
    public interface IConsulAclProvider
    {
        string GetAclToken();
    }
}
