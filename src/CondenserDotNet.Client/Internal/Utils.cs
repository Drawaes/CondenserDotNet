using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondenserDotNet.Client.Internal
{
    public static class Utils
    {
        public static bool DictionaryEquals<TKey, TValue>( this Dictionary<TKey, TValue> left, Dictionary<TKey, TValue> right)
        {
            var comp = EqualityComparer<TValue>.Default;
            if (left.Count != right.Count)
            {
                return false;
            }
            foreach (var pair in left)
            {
                TValue value;
                if (!right.TryGetValue(pair.Key, out value)
                     || !comp.Equals(pair.Value, value))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
