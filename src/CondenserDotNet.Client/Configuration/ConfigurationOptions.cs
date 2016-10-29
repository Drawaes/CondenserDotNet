using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondenserDotNet.Client.Configuration
{
    public class ConfigurationOptions
    {
        private ServiceManager _manager;
        private string _keyPath;

        public ConfigurationOptions(ServiceManager manager, string keyPath)
        {
            _keyPath = keyPath;
            _manager = manager;
        }

        public ServiceManager Manager => _manager;
        public string KeyPath => _keyPath;
    }
}
