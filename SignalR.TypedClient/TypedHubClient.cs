using Castle.DynamicProxy;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR.TypedClient
{
    public class TypedHubClient<THub> : IAsyncDisposable where THub : class
    {
        private HubConnection _hubConnection;

        public Task StartAsync(CancellationToken cancellationToken = default) => _hubConnection.StartAsync(cancellationToken);
        public Task StopAsync(CancellationToken cancellationToken = default) => _hubConnection.StopAsync(cancellationToken);
        public string ConnectionId => _hubConnection.ConnectionId;
        public HubConnectionState State => _hubConnection.State;
        public TimeSpan ServerTimeout
        {
            get => _hubConnection.ServerTimeout;
            set => _hubConnection.ServerTimeout = value;
        }
        public TimeSpan HandshakeTimeout
        {
            get => _hubConnection.HandshakeTimeout;
            set => _hubConnection.HandshakeTimeout = value;
        }
        public TimeSpan KeepAliveInterval
        {
            get => _hubConnection.KeepAliveInterval;
            set => _hubConnection.KeepAliveInterval = value;
        }
        public event Func<Exception, Task> Closed
        {
            add => _hubConnection.Closed += value;
            remove => _hubConnection.Closed -= value;
        }
        public event Func<string, Task> Reconnected
        {
            add => _hubConnection.Reconnected += value;
            remove => _hubConnection.Reconnected -= value;
        }
        public event Func<Exception, Task> Reconnecting
        {
            add => _hubConnection.Reconnecting += value;
            remove => _hubConnection.Reconnecting -= value;
        }
        public async ValueTask DisposeAsync()
        {
            await _hubConnection.DisposeAsync();
        }

        public IDisposable RegisterCallbacks<TCallbacks>(TCallbacks clientCallbacks) where TCallbacks : class
        {
            var methods = typeof(TCallbacks).GetMethods();
            var disposables = methods.Select(methodInfo =>
            {
                var parameterTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
                return _hubConnection.On(methodInfo.Name, parameterTypes, (parameters, _) => (Task)methodInfo.Invoke(clientCallbacks, parameters), null);
            }).ToArray();

            return new ListenerDisposable(disposables);
        }

        private class ListenerDisposable : IDisposable
        {
            private readonly IDisposable[] _disposables;

            public ListenerDisposable(IDisposable[] disposables)
            {
                _disposables = disposables;
            }

            public void Dispose()
            {
                foreach (var disposable in _disposables) disposable.Dispose();
            }
        }

        internal TypedHubClient(HubConnection hubConnection)
        {
            _hubConnection = hubConnection;
            var proxyGen = new ProxyGenerator();

            Invoke = proxyGen.CreateInterfaceProxyWithoutTarget<THub>(new InvokeInterceptor(hubConnection));
        }

        public THub Invoke { get; }
    }
}
