namespace CondenserDotNet.Services.Consul
{ 
    public class ServiceInformationSet
    {
        public NodeInformation Node { get;set;}
        public ServiceInformation Service { get;set;}
        public CheckInformation[] Checks { get;set;}
    }
}
