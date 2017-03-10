namespace CondenserDotNet.Core.Routing
{
    public interface IDefaultRouting<T>
    {
        IRoutingStrategy<T> Default { get; }
        void SetDefault(IRoutingStrategy<T> strateg);
    }
}