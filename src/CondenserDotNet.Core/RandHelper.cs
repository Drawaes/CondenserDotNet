using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CondenserDotNet.Core
{
    public static class RandHelper
    {
        private static int _seed = Environment.TickCount;
        private static readonly ThreadLocal<Random> s_random =
            new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));

        public static int Next(int lowerBound, int upperBound) => s_random.Value.Next(lowerBound, upperBound);
    }
}
