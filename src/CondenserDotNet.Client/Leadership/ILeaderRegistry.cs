
using System.Threading.Tasks;

namespace CondenserDotNet.Client.Leadership
{
    public interface ILeaderRegistry
    {
        /// <summary>
        /// Creates a session and starts watching the key
        /// </summary>
        /// <param name="keyForLeadership">The full path to the leadership key</param>
        /// <returns></returns>
        Task<ILeaderWatcher> GetLeaderWatcherAsync(string keyForLeadership);
    }
}
