[![Build status](https://ci.appveyor.com/api/projects/status/r2088yqbhp57cu66?svg=true)](https://ci.appveyor.com/project/Drawaes/condenserdotnet)
[![Xplat Build status](https://travis-ci.org/Drawaes/CondenserDotNet.svg?branch=master)](https://travis-ci.org/Drawaes/CondenserDotNet)

# CondenserDotNet

[![Join the chat at https://gitter.im/CondenserDotNet/Lobby](https://badges.gitter.im/CondenserDotNet/Lobby.svg)](https://gitter.im/CondenserDotNet/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
API Condenser / Reverse Proxy using Kestrel and Consul, Including light weight consul lib

A set of consul clients for .net that is simple and integrates with an API proxy

CI Builds available as nuget packs from 

https://www.myget.org/F/condenserdotnet/api/v3/index.json

Example use to register a service

``` csharp
var regClient = new ServiceRegistrationClient();
    regClient
        .Config(serviceName: "timsService", port:7777, address: "localhost")
        .AddSupportedVersions(new Version(1,0,0))
        .AddHealthCheck("Health", 10, 20);
    await regClient.RegisterServiceAsync();
```

