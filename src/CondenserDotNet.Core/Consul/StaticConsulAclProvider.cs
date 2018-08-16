using System;
using System.Collections.Generic;
using System.Text;

namespace CondenserDotNet.Core.Consul
{
    public class StaticConsulAclProvider : IConsulAclProvider
    {
        private readonly string _aclToken;

        public StaticConsulAclProvider(string aclToken) => _aclToken = aclToken;

        public string GetAclToken() => _aclToken;
    }
}
