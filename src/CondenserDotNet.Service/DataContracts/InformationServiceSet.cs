namespace CondenserDotNet.Service.DataContracts
{
    public class InformationServiceSet
    {
        public InformationNode Node { get;set;}
        public InformationService Service { get;set;}
        public InformationCheck[] Checks { get;set;}
    }
}
