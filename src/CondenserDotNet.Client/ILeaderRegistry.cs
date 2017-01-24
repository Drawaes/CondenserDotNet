using CondenserDotNet.Client.Internal;

namespace CondenserDotNet.Client
{
    public interface ILeaderRegistry
    {
        /// <summary>
        /// Creates a session and starts watching the key
        /// </summary>
        /// <param name="keyForLeadership">The full path to the leadership key</param>
        /// <returns></returns>
        ILeaderWatcher GetLeaderWatcher(string keyForLeadership);
    }
}