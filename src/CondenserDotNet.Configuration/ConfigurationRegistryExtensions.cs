using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CondenserDotNet.Configuration
{
    public static class ConfigurationRegistryExtensions
    {
        public static Task<bool> SetKeyJsonAsync<T>(this IConfigurationRegistry self, string key, T value)
        {
            return self.SetKeyAsync(key, JsonConvert.SerializeObject(value));
        }
    }
}