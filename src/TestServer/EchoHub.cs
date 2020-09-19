using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using TestCommon;

namespace TestServer
{
    public class EchoHub : Hub<IEchoHubClient>, IEchoHub
    {
        public async Task<string> Echo(string message)
        {
            await Clients.All.OnMessageReceived(message);
            return message;
        }
    }
}
