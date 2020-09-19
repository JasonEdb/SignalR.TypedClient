# SignalR.TypedClient
A proof of concept for strongly typed SignalRClients using `Castle.DynamicProxy`

![Intellisense on hub client](images/StronglyTyped.png)

## Client Code

```csharp
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using SignalR.TypedClient;
using TestCommon;

namespace TestClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HubConnectionBuilder();
            await Task.Delay(TimeSpan.FromSeconds(3));
            var hub = builder
                .WithUrl("http://localhost:5000/echo")
                .WithAutomaticReconnect()
                .Build<IEchoHub>();

            var unregister = hub.RegisterCallbacks(new MyClientCallbacks());
            await hub.StartAsync();
            var message = await hub.Invoke.Echo("Some message");
            Console.WriteLine($"Result: {message}");

        }
    }

    public class MyClientCallbacks : IEchoHubClient
    {
        public Task OnMessageReceived(string message)
        {
            Console.WriteLine($"{nameof(OnMessageReceived)} - {message}");
            return Task.CompletedTask;
        }
    }
}

```
