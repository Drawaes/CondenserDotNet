﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CondenserDotNet.Client.Services
{
    public class NoServiceInstanceFoundException:Exception
    {
        public NoServiceInstanceFoundException(string serviceName, Exception innerException)
            :base($"Unable to find an instance of the service {serviceName}", innerException)
        {
            ServiceName = serviceName;
        }

        public string ServiceName { get;}

        public override string ToString()
        {
            return Message;
        }
    }
}
