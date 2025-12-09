using System.Net;
using System.Net.Sockets;

namespace Infra.Helpers
{
    public static class HostHelpers
    {
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList?
                .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork)?
                .ToString();

            if (!string.IsNullOrEmpty(host))
            {
                return host;
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
