namespace CondenserDotNet.Server
{
    public interface IConsulService : IService
    {
        void Initialise(string serviceId,
            string nodeId, string[] tags,
            string address, int port);
    }
}