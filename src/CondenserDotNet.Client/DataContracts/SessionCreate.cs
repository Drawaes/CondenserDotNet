namespace CondenserDotNet.Client.DataContracts
{
    public class SessionCreate
    {
        public string LockDelay { get; set; }
        public string Name { get; set; }
        public string[] Checks { get; set; }
        public string Behavior { get; set; }
    }
}
