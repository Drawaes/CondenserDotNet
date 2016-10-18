using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondenserDotNet.Host.RoutingTrie
{
    public class NodeComparer: IEqualityComparer<string[]>
    {
        private int _compareLength;
        public NodeComparer(int compareLength)
        {
            _compareLength = compareLength;
        }

        public int CompareLength { get { return _compareLength; } }

        public bool Equals(string[] x, string[] y)
        {
            if(y.Length < _compareLength || x.Length < _compareLength)
                return false;

            for(int i = 0; i < _compareLength; i++)
            {
                if(x[i] != y[i])
                    return false;                
            }

            return true;
        }

        public int GetHashCode(string[] obj)
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                for(int i =0; i < _compareLength; i++)
                {
                    hash = hash * 23 + obj[i].GetHashCode();
                }
                return hash;
            }
        }
    }
}
