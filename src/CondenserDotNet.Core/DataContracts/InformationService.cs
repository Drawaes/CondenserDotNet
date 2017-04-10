using System;

namespace CondenserDotNet.Core.DataContracts
{
    public class InformationService : IEquatable<InformationService>
    {
        public string ID { get; set; }
        public string Service { get; set; }
        public string[] Tags { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }

        public bool Equals(InformationService other)
        {
            if (Address != other.Address) return false;
            if (Port != other.Port) return false;
            if (Service != other.Service) return false;
            if (ID != other.ID) return false;
            return true;
        }
    }
}
