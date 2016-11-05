[![Build status](https://ci.appveyor.com/api/projects/status/r2088yqbhp57cu66?svg=true)](https://ci.appveyor.com/project/Drawaes/condenserdotnet)
[![Xplat Build status](https://travis-ci.org/Drawaes/CondenserDotNet.svg?branch=master)](https://travis-ci.org/Drawaes/CondenserDotNet)
[![Join the chat at https://gitter.im/CondenserDotNet/Lobby](https://badges.gitter.im/CondenserDotNet/Lobby.svg)](https://gitter.im/CondenserDotNet/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![Coveralls](https://img.shields.io/coveralls/Drawaes/CondenserDotNet.svg)](https://coveralls.io/github/Drawaes/CondenserDotNet?branch=master)
# CondenserDotNet

API Condenser / Reverse Proxy using Kestrel and Consul, Including light weight consul lib

A set of consul clients for .net that is simple and integrates with an API proxy

CI Builds available as nuget packs from myget

[![MyGet](https://img.shields.io/myget/condenserdotnet/v/CondenserDotNet.Client.svg)](https://www.myget.org/F/condenserdotnet/api/v3/index.json)

Current release is available at

[![NuGet](https://img.shields.io/nuget/v/CondenserDotNet.Client.svg)](https://www.nuget.org/packages/CondenserDotNet.Client/)

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

## Example to get an instance of a service to connect to

The following gets an instance of a service to call. If you recall the method you will get randomly rotated around the instances and they will be updated in the background based on the health changing

``` csharp
var serviceInstance = await manager.Services.GetServiceInstanceAsync("ServiceLookup");
if(serviceInsance == null)
{
	//you need to handle no service instance available
}
//connect to service via your method
var serverUrl = $"http://{serviceInstance.Address}:{serviceInsance.Port}";

serviceInstance2 = await manager.Services.GetServiceInstanceAsync("ServiceLookup");
```

## Example for configuration

Configuration follows a LIFO policy, in that the last configuration item registered will "win" and override the same keys from a previous registration. Both static and dynamic registrations will override each other. To access values you call

``` csharp
var configValue = serviceManager.Config["MyKey"];
```
If the key doesn't exist it will throw an exception. You can access it with the normal tryget pattern as well
``` csharp
string configValue;
if(serviceManager.Config.TryGetValue("MyKey", out configValue))
{
	//do something with the key
}
```

### Static configuration

To register a static set of keys you call the following
``` csharp
var result = await ServiceManager.Config.AddStaticKeyPathAsync("my/keys/path");
```
result will be false if the key bucket or key was not found otherwise it will return true and add all of the child keys recursively. Keys will be in the format keyPath:keyPath:Key in the config after that to comply with the ASP.Net configuration. It will not include the path prefix that you used in adding the configuraiton (in this case "my/key/path")

### Dynamic configuration

You can register a key path for dynamic configuration. This will watch consul for any changes in a a callback fashion. It uses async long polling so it should react instantly and without causing undue network traffic or system load.
Below is how you register a key space that you want the library to watch until disposal.

``` csharp
var manager = new ServiceManager("TestService");
    await manager.Config.AddUpdatingPathAsync("org/test5/");
```

You can also register a callback to alert you if a specific key is updated or if any key in the config is updated. The any key might be triggered even if there is no actual effective update (due to an override) so applications will need to check if their information has actually been updated.
The single key watch will check for an actual update and only return if that key has changed.

``` csharp
var singleCallBack = manager.Config.AddWatchOnSingleKey("test1", () => Console.Writeline("Key Changed!");
```

If you want any update to trigger your callback just do

``` csharp
var multipleCallBack = manager.Config.AddWatchOnEntireConfig(
    () => Console.Writeline("Some key changed, or multiple keys changed, or maybe none?");
```

### Leader Election

You can participate in leader election using the consul sessions. You simply ask for a LeaderWatcher based on a predefined key for your application.

``` csharp
var watcher1 = manager.Leaders.GetLeaderWatcher(leadershipKey);
```

You can then await on this object until a leader is elected and it will return when someone is elected and tell you who it is.

``` csharp
var leader = await watcher1.GetCurrentLeaderAsync();
```

If you would like to get the leadership you can do so, the GetLeadershipAsync will return once you have become leader. You can call this over and over to ensure that you haven't lost the leadership
``` csharp
while(true)
{
  await watcher1.GetLeadershipAsync();
  //Do some work that only the leader should do
  //then loop back to do the next bit of work
  //but check that we are still the leader
}
```