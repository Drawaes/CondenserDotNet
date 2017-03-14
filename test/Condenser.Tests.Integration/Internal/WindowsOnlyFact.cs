using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Condenser.Tests.Integration.Internal
{
    public class WindowsOnlyFact: Xunit.FactAttribute
    {
        public WindowsOnlyFact()
        {
            if(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Skip = "Windows Only Test";
            }
        }
    }
}
