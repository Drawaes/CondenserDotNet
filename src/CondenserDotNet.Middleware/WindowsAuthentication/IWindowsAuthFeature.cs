using System.Security.Principal;

namespace CondenserDotNet.Middleware.WindowsAuthentication
{
    public interface IWindowsAuthFeature
    {
        WindowsIdentity Identity { get; set; }
        WindowsIdentity GetUser();
        string ProcessHandshake(string tokenName, byte[] token);
    }
}