using CondenserDotNet.Client.Internal;

namespace CondenserDotNet.Client
{
    public interface ILeaderRegistry
    {
        ILeaderWatcher GetLeaderWatcher(string keyForLeadership);
    }
}