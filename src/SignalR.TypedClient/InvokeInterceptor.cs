using Castle.DynamicProxy;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SignalR.TypedClient
{
    internal class InvokeInterceptor : StandardInterceptor
    {
        private HubConnection _hubConnection;

        public InvokeInterceptor(HubConnection hubConnection)
        {
            _hubConnection = hubConnection;
        }

        protected override void PerformProceed(IInvocation invocation)
        {
            var methodInfo = invocation.Method;

            if (methodInfo.ReturnType.IsGenericType)
            {
                var returnType = methodInfo.ReturnType.GetGenericArguments().Single();
                var genericResult = _hubConnection.InvokeCoreAsync(methodInfo.Name, returnType, invocation.Arguments);

                var sourceType = typeof(TaskCompletionSource<>).MakeGenericType(returnType);
                var taskCompletionSource = Activator.CreateInstance(sourceType);

                Task.Run(async () =>
                {
                    try
                    {
                        var result = await genericResult;
                        sourceType.GetMethod(nameof(TaskCompletionSource<object>.TrySetResult)).Invoke(taskCompletionSource, new[] { result });
                    }
                    catch (OperationCanceledException)
                    {
                        sourceType.GetMethod(nameof(TaskCompletionSource<object>.TrySetCanceled)).Invoke(taskCompletionSource, null);
                    }
                    catch (Exception e)
                    {
                        sourceType.GetMethod(nameof(TaskCompletionSource<object>.TrySetException)).Invoke(taskCompletionSource, new[] { e });
                    }
                });
                invocation.ReturnValue = sourceType.GetProperty(nameof(TaskCompletionSource<object>.Task)).GetValue(taskCompletionSource);
            }
            else
            {
                var result = _hubConnection.InvokeCoreAsync(methodInfo.Name, invocation.Arguments);
                invocation.ReturnValue = result;
            }
        }
    }
}
