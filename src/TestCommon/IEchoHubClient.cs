using System.Threading.Tasks;

namespace TestCommon
{
    public interface IEchoHubClient
    {
        Task OnMessageReceived(string message);
    }
}
