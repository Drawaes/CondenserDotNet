[![Build status](https://ci.appveyor.com/api/projects/status/r2088yqbhp57cu66?svg=true)](https://ci.appveyor.com/project/Drawaes/condenserdotnet)
[![Xplat Build status](https://travis-ci.org/Drawaes/CondenserDotNet.svg?branch=master)](https://travis-ci.org/Drawaes/CondenserDotNet)
[![Join the chat at https://gitter.im/CondenserDotNet/Lobby](https://badges.gitter.im/CondenserDotNet/Lobby.svg)](https://gitter.im/CondenserDotNet/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

# CondenserDotNet

API Condenser / Reverse Proxy using Kestrel and Consul, Including light weight consul lib

A set of consul clients for .net that is simple and integrates with an API proxy

CI Builds available as nuget packs from 

https://www.myget.org/F/condenserdotnet/api/v3/index.json

## Example use to register a service

``` csharp
var serviceManager = new ServiceManager("TestService");
    await serviceManager.AddHttpHealthCheck("health",10)
        .AddApiUrl("/api/someObject")
        .AddApiUrl("/api/someOtherObject")
        .RegisterServiceAsync();
```

## Example to configure Kestrel on a dynamic port

``` csharp
var host = new WebHostBuilder()
    .UseKestrel()
    .UseUrls($"http://*:{serviceManager.ServicePort}")
    .UseStartup<Startup>()
    .Build();

host.Run();
```

The first available port in the dynamic range for windows is allocated by default. You can override this if you have a specific port you would like to use.  

``` csharp
serviceManager.ServicePort = 5000;
```

You should assign this before registering or call RegisterServiceAsync() again if you change the details to send the new configuration to Consul.
