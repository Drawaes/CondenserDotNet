using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CondenserDotNet.Client.Configuration
{
    public static class ConfigurationExtensions
    {
        public static IConfigurationBuilder AddServiceConfiguration(this IConfigurationBuilder builder, Action<ConfigurationOptions> options)
        {
            ConfigurationSource newBuilder = new ConfigurationSource(options);
            return builder.Add(newBuilder);
        }
    }
}
