# Service Registration

The client library for CondenserDotNet provides a number of services. The service registration makes up one of the primary uses.
The following is some examples of how this can be used.

## Simple Service Registration
The following registers a service with the service name "TestService". The service Id will be set to "TestService:{hostname}".

``` csharp
var serviceManager = new ServiceManager("TestService");
await serviceManager.RegisterServiceAsync();
```

Service Id's are required to be unique for a single Consul agent. If you are running multiple instances of a single service
against an agent you will need to give it a service Id.

``` csharp
var serviceManager = new ServiceManager("TestService", $"TestService{Guid.New().ToString()}");
await serviceManager;
```

Giving the service a Guid in it's Id will ensure it is unique but you can use any scheme you wish.

## Service Registration, agent on another machine

There are scenarios where you will need to run the agent on a seperate machine, or different port you can do this with the following.

``` csharp
var serviceManager = new ServiceManager("TestService","consulAgentMachine",8500);
await serviceManager.RegisterServiceAsync();
```

You will need to add the port as well as the machine even if the port is the default otherwise you will clash with the serviceId override.
If you also wish to override the serviceId your constructor would look like this.

``` csharp
var serviceManager = new ServiceManager("TestService", "instanceName", "agentMachine", 8500);
await serviceManager.RegisterServiceAsync();
```


