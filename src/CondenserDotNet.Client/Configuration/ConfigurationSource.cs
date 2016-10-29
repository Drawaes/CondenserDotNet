using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CondenserDotNet.Client.Configuration
{
    public class ConfigurationSource : IConfigurationSource
    {
        private readonly Action<ConfigurationOptions> _optionsAction;

        public ConfigurationSource(Action<ConfigurationOptions> optionsAction)
        {
            _optionsAction = optionsAction;
        }
                
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            throw new NotImplementedException();
        }
    }
}
