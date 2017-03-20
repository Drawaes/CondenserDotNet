using System.Runtime.InteropServices;

namespace Condenser.Tests.Integration.Internal
{
    public class WindowsOnlyFact : Xunit.FactAttribute
    {
        public WindowsOnlyFact()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Skip = "Windows Only Test";
            }
        }
    }
}
