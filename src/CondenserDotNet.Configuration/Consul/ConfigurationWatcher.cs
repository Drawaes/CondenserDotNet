using System;

namespace CondenserDotNet.Configuration.Consul
{
    internal class ConfigurationWatcher
    {
        public string CurrentValue { get; set; }
        public Action<string> CallBack { get; set; }
        public Action CallbackAllKeys { get; set; }
        public string KeyToWatch { get; set; }
    }
}
