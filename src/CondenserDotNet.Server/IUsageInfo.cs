namespace CondenserDotNet.Server
{
    public interface IUsageInfo
    {
        int Calls { get; }
        double TotalRequestTime { get; }
    }
}