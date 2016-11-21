using System.Reflection;
using System.Threading.Tasks;
using RSB.Interfaces;
using RSB.Modules.Templater.Common.Contracts;
using RSB.Modules.Templater.Common.Utils;

namespace RSB.Modules.Templater.Common
{
    public class TemplaterService
    {
        private readonly IBus _bus;
        private readonly string _routingKey;

        public TemplaterService(IBus bus, string routingKey)
        {
            _bus = bus;
            _routingKey = routingKey;
        }

        public async Task<string> FillTemplateAsync<T>(T contract) where T : new()
        {
            var request = ReflectionUtils.InstantiateTemplateRequest<T>();
            request.Variables = contract;

            var responseType = ReflectionUtils.BuildDynamicResponseType<T>();

            var callRpcMethod = typeof(TemplaterService).GetMethod(nameof(CallRpc), BindingFlags.Instance | BindingFlags.NonPublic);
            var callRpcGeneric = callRpcMethod.MakeGenericMethod(request.GetType(), responseType, contract.GetType());
            var task = (Task<ITemplateResponse<T>>)callRpcGeneric.Invoke(this, new object[] { request });

            var response = await task;
            return response.Text;
        }

        private async Task<ITemplateResponse<T>> CallRpc<TRequest, TResponse, T>(TRequest request)
            where TRequest : ITemplateRequest<T>, new()
            where TResponse : ITemplateResponse<T>, new()
        {
            return await _bus.Call<TRequest, TResponse>(request, _routingKey);
        }
    }
}