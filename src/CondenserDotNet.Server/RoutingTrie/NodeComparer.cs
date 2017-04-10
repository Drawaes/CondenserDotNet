using System;
using System.Collections.Generic;

namespace CondenserDotNet.Server.RoutingTrie
{
    public class NodeComparer : IEqualityComparer<string[]>
    {
        private readonly int _compareLength;

        public NodeComparer(int compareLength) => _compareLength = compareLength;

        public int CompareLength => _compareLength;

        public bool Equals(string[] x, string[] y)
        {
            if (y.Length < _compareLength || x.Length < _compareLength)
            {
                return false;
            }

            for (var i = 0; i < _compareLength; i++)
            {
                if (x[i] != y[i]) return false;
            }
            return true;
        }

        public int GetHashCode(string[] obj)
        {
            unchecked // Overflow is fine, just wrap
            {
                var hash = 17;
                for (var i = 0; i < Math.Min(_compareLength, obj.Length); i++)
                {
                    hash = hash * 23 + obj[i].GetHashCode();
                }
                return hash;
            }
        }
    }
}
