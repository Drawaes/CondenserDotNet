namespace CondenserDotNet.Services.Consul
{
    internal enum WatcherState
    {
        NotInitialized,
        UsingCachedValues,
        UsingLiveValues
    }
}