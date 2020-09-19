using System.Threading.Tasks;

namespace TestCommon
{
    public interface IEchoHub
    {
        Task<string> Echo(string message);
    }
}
