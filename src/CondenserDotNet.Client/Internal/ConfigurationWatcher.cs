﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondenserDotNet.Client.Internal
{
    internal class ConfigurationWatcher
    {
        public string CurrentValue { get; set; }
        public Action<string> CallBack { get; set; }
        public Action CallbackAllKeys { get;set;}
        public string KeyToWatch { get; set; }
    }
}
