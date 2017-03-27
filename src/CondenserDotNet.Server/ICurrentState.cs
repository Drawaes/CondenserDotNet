namespace CondenserDotNet.Server
{
    public interface ICurrentState : ISummary
    {
        void RecordResponse(int responseCode);
        
    }

    public interface ISummary
    {
        CurrentState.Summary GetSummary();
    }
}
