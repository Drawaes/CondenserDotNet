using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using CondenserDotNet.Client.DataContracts;
using CondenserDotNet.Core;

namespace CondenserDotNet.Client
{
    public interface IServiceManager : IDisposable
    {
            
       
        
        ILeaderRegistry Leaders { get; }
               
        

        
      

    }
}