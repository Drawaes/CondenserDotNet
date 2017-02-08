
var camelCaseTokenizer = function (obj) {
    var previous = '';
    return obj.toString().trim().split(/[\s\-]+|(?=[A-Z])/).reduce(function(acc, cur) {
        var current = cur.toLowerCase();
        if(acc.length === 0) {
            previous = current;
            return acc.concat(current);
        }
        previous = previous.concat(current);
        return acc.concat([current, previous]);
    }, []);
}
lunr.tokenizer.registerFunction(camelCaseTokenizer, 'camelCaseTokenizer')
var searchModule = function() {
    var idMap = [];
    function y(e) { 
        idMap.push(e); 
    }
    var idx = lunr(function() {
        this.field('title', { boost: 10 });
        this.field('content');
        this.field('description', { boost: 5 });
        this.field('tags', { boost: 50 });
        this.ref('id');
        this.tokenizer(camelCaseTokenizer);

        this.pipeline.remove(lunr.stopWordFilter);
        this.pipeline.remove(lunr.stemmer);
    });
    function a(e) { 
        idx.add(e); 
    }

    a({
        id:0,
        title:"HealthCheck",
        content:"HealthCheck",
        description:'',
        tags:''
    });

    a({
        id:1,
        title:"InformationCheck",
        content:"InformationCheck",
        description:'',
        tags:''
    });

    a({
        id:2,
        title:"JsonKeyValueParser",
        content:"JsonKeyValueParser",
        description:'',
        tags:''
    });

    a({
        id:3,
        title:"ConsulProvider",
        content:"ConsulProvider",
        description:'',
        tags:''
    });

    a({
        id:4,
        title:"ServiceInstance",
        content:"ServiceInstance",
        description:'',
        tags:''
    });

    a({
        id:5,
        title:"IHealthConfig",
        content:"IHealthConfig",
        description:'',
        tags:''
    });

    a({
        id:6,
        title:"ServiceCollectionExtentions",
        content:"ServiceCollectionExtentions",
        description:'',
        tags:''
    });

    a({
        id:7,
        title:"ILeaderRegistry",
        content:"ILeaderRegistry",
        description:'',
        tags:''
    });

    a({
        id:8,
        title:"AuthenticationConnectionFilter",
        content:"AuthenticationConnectionFilter",
        description:'',
        tags:''
    });

    a({
        id:9,
        title:"RouteSummary",
        content:"RouteSummary",
        description:'',
        tags:''
    });

    a({
        id:10,
        title:"WindowsAuthHandshakeCache",
        content:"WindowsAuthHandshakeCache",
        description:'',
        tags:''
    });

    a({
        id:11,
        title:"Node",
        content:"Node",
        description:'',
        tags:''
    });

    a({
        id:12,
        title:"IKeyParser",
        content:"IKeyParser",
        description:'',
        tags:''
    });

    a({
        id:13,
        title:"CondenserConfiguration",
        content:"CondenserConfiguration",
        description:'',
        tags:''
    });

    a({
        id:14,
        title:"WindowsAuthFeature",
        content:"WindowsAuthFeature",
        description:'',
        tags:''
    });

    a({
        id:15,
        title:"IConfigurationRegistry",
        content:"IConfigurationRegistry",
        description:'',
        tags:''
    });

    a({
        id:16,
        title:"NodeContainer",
        content:"NodeContainer",
        description:'',
        tags:''
    });

    a({
        id:17,
        title:"IServiceManager",
        content:"IServiceManager",
        description:'',
        tags:''
    });

    a({
        id:18,
        title:"ConfigurationRegistryExtensions",
        content:"ConfigurationRegistryExtensions",
        description:'',
        tags:''
    });

    a({
        id:19,
        title:"InformationService",
        content:"InformationService",
        description:'',
        tags:''
    });

    a({
        id:20,
        title:"IConfigurationBuilder",
        content:"IConfigurationBuilder",
        description:'',
        tags:''
    });

    a({
        id:21,
        title:"RadixTree",
        content:"RadixTree",
        description:'',
        tags:''
    });

    a({
        id:22,
        title:"NodeComparer",
        content:"NodeComparer",
        description:'',
        tags:''
    });

    a({
        id:23,
        title:"CustomRouter",
        content:"CustomRouter",
        description:'',
        tags:''
    });

    a({
        id:24,
        title:"WindowsHandshake",
        content:"WindowsHandshake",
        description:'',
        tags:''
    });

    a({
        id:25,
        title:"CurrentState ThreadStats",
        content:"CurrentState ThreadStats",
        description:'',
        tags:''
    });

    a({
        id:26,
        title:"ConfigurationBuilder",
        content:"ConfigurationBuilder",
        description:'',
        tags:''
    });

    a({
        id:27,
        title:"WindowsAuthenticationMiddleware",
        content:"WindowsAuthenticationMiddleware",
        description:'',
        tags:''
    });

    a({
        id:28,
        title:"RoutingHost",
        content:"RoutingHost",
        description:'',
        tags:''
    });

    a({
        id:29,
        title:"ChildContainer",
        content:"ChildContainer",
        description:'',
        tags:''
    });

    a({
        id:30,
        title:"ConsulWatcher",
        content:"ConsulWatcher",
        description:'',
        tags:''
    });

    a({
        id:31,
        title:"HttpUtils",
        content:"HttpUtils",
        description:'',
        tags:''
    });

    a({
        id:32,
        title:"WindowsAuthStreamWrapper",
        content:"WindowsAuthStreamWrapper",
        description:'',
        tags:''
    });

    a({
        id:33,
        title:"InformationNode",
        content:"InformationNode",
        description:'',
        tags:''
    });

    a({
        id:34,
        title:"LeaderRegistry",
        content:"LeaderRegistry",
        description:'',
        tags:''
    });

    a({
        id:35,
        title:"ServiceBase",
        content:"ServiceBase",
        description:'',
        tags:''
    });

    a({
        id:36,
        title:"ILeaderWatcher",
        content:"ILeaderWatcher",
        description:'',
        tags:''
    });

    a({
        id:37,
        title:"ServiceCollectionExtensions",
        content:"ServiceCollectionExtensions",
        description:'',
        tags:''
    });

    a({
        id:38,
        title:"AsyncManualResetEvent",
        content:"AsyncManualResetEvent",
        description:'',
        tags:''
    });

    a({
        id:39,
        title:"HealthRouter",
        content:"HealthRouter",
        description:'',
        tags:''
    });

    a({
        id:40,
        title:"ConfigurationRegistry",
        content:"ConfigurationRegistry",
        description:'',
        tags:''
    });

    a({
        id:41,
        title:"ServiceManager",
        content:"ServiceManager",
        description:'',
        tags:''
    });

    a({
        id:42,
        title:"ConsulSource",
        content:"ConsulSource",
        description:'',
        tags:''
    });

    a({
        id:43,
        title:"HealthResponse",
        content:"HealthResponse",
        description:'',
        tags:''
    });

    a({
        id:44,
        title:"CurrentState",
        content:"CurrentState",
        description:'',
        tags:''
    });

    a({
        id:45,
        title:"Service",
        content:"Service",
        description:'',
        tags:''
    });

    a({
        id:46,
        title:"HealthCheck",
        content:"HealthCheck",
        description:'',
        tags:''
    });

    a({
        id:47,
        title:"ConsulConfigurationExtensions",
        content:"ConsulConfigurationExtensions",
        description:'',
        tags:''
    });

    a({
        id:48,
        title:"KeyValue",
        content:"KeyValue",
        description:'',
        tags:''
    });

    a({
        id:49,
        title:"TtlCheck",
        content:"TtlCheck",
        description:'',
        tags:''
    });

    a({
        id:50,
        title:"RoutingData",
        content:"RoutingData",
        description:'',
        tags:''
    });

    a({
        id:51,
        title:"BlockingWatcher",
        content:"BlockingWatcher",
        description:'',
        tags:''
    });

    a({
        id:52,
        title:"LeaderWatcher",
        content:"LeaderWatcher",
        description:'',
        tags:''
    });

    a({
        id:53,
        title:"ITtlCheck",
        content:"ITtlCheck",
        description:'',
        tags:''
    });

    a({
        id:54,
        title:"SessionCreate",
        content:"SessionCreate",
        description:'',
        tags:''
    });

    a({
        id:55,
        title:"RegistrationExtensions",
        content:"RegistrationExtensions",
        description:'',
        tags:''
    });

    a({
        id:56,
        title:"Node",
        content:"Node",
        description:'',
        tags:''
    });

    a({
        id:57,
        title:"TreeRouter",
        content:"TreeRouter",
        description:'',
        tags:''
    });

    a({
        id:58,
        title:"InformationServiceSet",
        content:"InformationServiceSet",
        description:'',
        tags:''
    });

    a({
        id:59,
        title:"IServiceRegistry",
        content:"IServiceRegistry",
        description:'',
        tags:''
    });

    a({
        id:60,
        title:"Service",
        content:"Service",
        description:'',
        tags:''
    });

    a({
        id:61,
        title:"ServiceRegistry",
        content:"ServiceRegistry",
        description:'',
        tags:''
    });

    a({
        id:62,
        title:"ApplicationBuilderExtensions",
        content:"ApplicationBuilderExtensions",
        description:'',
        tags:''
    });

    a({
        id:63,
        title:"WindowsAuthenticationExtensions",
        content:"WindowsAuthenticationExtensions",
        description:'',
        tags:''
    });

    a({
        id:64,
        title:"HealthCheckStatus",
        content:"HealthCheckStatus",
        description:'',
        tags:''
    });

    a({
        id:65,
        title:"WebHostExtensions",
        content:"WebHostExtensions",
        description:'',
        tags:''
    });

    a({
        id:66,
        title:"IService",
        content:"IService",
        description:'',
        tags:''
    });

    a({
        id:67,
        title:"SessionCreateResponse",
        content:"SessionCreateResponse",
        description:'',
        tags:''
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.DataContracts/HealthCheck',
        title:"HealthCheck",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Core.DataContracts/InformationCheck',
        title:"InformationCheck",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client.Configuration/JsonKeyValueParser',
        title:"JsonKeyValueParser",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client.Configuration/ConsulProvider',
        title:"ConsulProvider",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.DataContracts/ServiceInstance',
        title:"ServiceInstance",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.Builder/IHealthConfig',
        title:"IHealthConfig",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.Extensions/ServiceCollectionExtentions',
        title:"ServiceCollectionExtentions",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client/ILeaderRegistry',
        title:"ILeaderRegistry",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.WindowsAuthentication/AuthenticationConnectionFilter',
        title:"AuthenticationConnectionFilter",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.Routes/RouteSummary',
        title:"RouteSummary",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.WindowsAuthentication/WindowsAuthHandshakeCache',
        title:"WindowsAuthHandshakeCache",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.DataContracts/Node',
        title:"Node",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client.Configuration/IKeyParser',
        title:"IKeyParser",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server/CondenserConfiguration',
        title:"CondenserConfiguration",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.WindowsAuthentication/WindowsAuthFeature',
        title:"WindowsAuthFeature",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client/IConfigurationRegistry',
        title:"IConfigurationRegistry",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.RoutingTrie/NodeContainer_1',
        title:"NodeContainer<T>",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client/IServiceManager',
        title:"IServiceManager",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client/ConfigurationRegistryExtensions',
        title:"ConfigurationRegistryExtensions",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Core.DataContracts/InformationService',
        title:"InformationService",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.Builder/IConfigurationBuilder',
        title:"IConfigurationBuilder",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.RoutingTrie/RadixTree_1',
        title:"RadixTree<T>",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.RoutingTrie/NodeComparer',
        title:"NodeComparer",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server/CustomRouter',
        title:"CustomRouter",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.WindowsAuthentication/WindowsHandshake',
        title:"WindowsHandshake",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.Routes/ThreadStats',
        title:"CurrentState.ThreadStats",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.Builder/ConfigurationBuilder',
        title:"ConfigurationBuilder",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.WindowsAuthentication/WindowsAuthenticationMiddleware',
        title:"WindowsAuthenticationMiddleware",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server/RoutingHost',
        title:"RoutingHost",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.RoutingTrie/ChildContainer_1',
        title:"ChildContainer<T>",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.Consul/ConsulWatcher',
        title:"ConsulWatcher",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Core/HttpUtils',
        title:"HttpUtils",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.WindowsAuthentication/WindowsAuthStreamWrapper',
        title:"WindowsAuthStreamWrapper",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Core.DataContracts/InformationNode',
        title:"InformationNode",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client/LeaderRegistry',
        title:"LeaderRegistry",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server/ServiceBase',
        title:"ServiceBase",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client.Internal/ILeaderWatcher',
        title:"ILeaderWatcher",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client/ServiceCollectionExtensions',
        title:"ServiceCollectionExtensions",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Core/AsyncManualResetEvent_1',
        title:"AsyncManualResetEvent<T>",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.Routes/HealthRouter',
        title:"HealthRouter",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client/ConfigurationRegistry',
        title:"ConfigurationRegistry",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client/ServiceManager',
        title:"ServiceManager",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client.Configuration/ConsulSource',
        title:"ConsulSource",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.Routes/HealthResponse',
        title:"HealthResponse",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.Routes/CurrentState',
        title:"CurrentState",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client.DataContracts/Service',
        title:"Service",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client.DataContracts/HealthCheck',
        title:"HealthCheck",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client.Configuration/ConsulConfigurationExtensions',
        title:"ConsulConfigurationExtensions",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client.DataContracts/KeyValue',
        title:"KeyValue",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client/TtlCheck',
        title:"TtlCheck",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server/RoutingData',
        title:"RoutingData",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Core/BlockingWatcher_1',
        title:"BlockingWatcher<T>",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client.Internal/LeaderWatcher',
        title:"LeaderWatcher",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client/ITtlCheck',
        title:"ITtlCheck",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client.DataContracts/SessionCreate',
        title:"SessionCreate",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client/RegistrationExtensions',
        title:"RegistrationExtensions",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.RoutingTrie/Node_1',
        title:"Node<T>",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.Routes/TreeRouter',
        title:"TreeRouter",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Core.DataContracts/InformationServiceSet',
        title:"InformationServiceSet",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Core/IServiceRegistry',
        title:"IServiceRegistry",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server/Service',
        title:"Service",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Core/ServiceRegistry',
        title:"ServiceRegistry",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.Extensions/ApplicationBuilderExtensions',
        title:"ApplicationBuilderExtensions",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.WindowsAuthentication/WindowsAuthenticationExtensions',
        title:"WindowsAuthenticationExtensions",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.DataContracts/HealthCheckStatus',
        title:"HealthCheckStatus",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server.Extensions/WebHostExtensions',
        title:"WebHostExtensions",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Server/IService',
        title:"IService",
        description:""
    });

    y({
        url:'/CondenserDotNet/CondenserDotNet/api/CondenserDotNet.Client.DataContracts/SessionCreateResponse',
        title:"SessionCreateResponse",
        description:""
    });

    return {
        search: function(q) {
            return idx.search(q).map(function(i) {
                return idMap[i.ref];
            });
        }
    };
}();
