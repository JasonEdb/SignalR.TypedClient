using Microsoft.AspNetCore.SignalR.Client;

namespace SignalR.TypedClient
{
    public static class IHubConnectionBuilderExtensions
    {
        public static TypedHubClient<THub> Build<THub>(this IHubConnectionBuilder connectionBuilder) where THub : class
        {
            return new TypedHubClient<THub>(connectionBuilder.Build());
        }
    }
}
