using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CondenserDotNet.Client.Leadership;
using CondenserDotNet.Client.Services;
using CondenserDotNet.Core;
using CondenserDotNet.Core.Consul;
using Microsoft.Extensions.DependencyInjection;

namespace CondenserDotNet.Client
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConsulServices(this IServiceCollection self)
        {
            self.AddSingleton<ILeaderRegistry, LeaderRegistry>();
            self.AddSingleton<IServiceRegistry, ServiceRegistry>();
            self.AddSingleton<IServiceManager, ServiceManager>();
            self.AddTransient<ServiceRegistryDelegatingHandler>();
            self.AddTransient<ServiceRegistryNearestDelegatingHandler>();
            return self;
        }

        public static IServiceCollection AddConsulServices(this IServiceCollection self, string aclToken)
        {
            self.AddSingleton<IConsulAclProvider>(new StaticConsulAclProvider(aclToken));
            return self.AddConsulServices();
        }

        public static IServiceCollection AddConsulServices(this IServiceCollection self, string encryptedAclTokenKey, X509Certificate2 encryptionCertifcate, RSAEncryptionPadding padding)
        {
            self.AddSingleton<IConsulAclProvider>(new EncryptedConsulKeyAclProvider(encryptedAclTokenKey, encryptionCertifcate, padding));
            return self.AddConsulServices();
        }
    }
}
