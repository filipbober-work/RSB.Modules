using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using RSB.Interfaces;
using RSB.Modules.Templater.Common.Contracts;
using RSB.Modules.Templater.Common.Utils;

namespace RSB.Modules.Templater.Common
{
    public class TemplaterService : ITemplaterService
    {
        private readonly IBus _bus;
        private readonly string _routingKey;

        private readonly Dictionary<string, Type> _responseTypes = new Dictionary<string, Type>();
        private readonly Dictionary<string, Type> _requestTypes = new Dictionary<string, Type>();

        public TemplaterService(IBus bus, string routingKey)
        {
            _bus = bus;
            _routingKey = routingKey;
        }

        public async Task<string> FillTemplateAsync<T>(T contract) where T : new()
        {
            var requestKey = ReflectionUtils.GetRequestName(contract.GetType());
            var responseKey = ReflectionUtils.GetResponseName(contract.GetType());

            Type requestType;
            ITemplateRequest<T> request;
            if (_requestTypes.TryGetValue(requestKey, out requestType))
            {
                request = ReflectionUtils.InstantiateCachedRequest<T>(requestType);
            }
            else
            {
                request = ReflectionUtils.InstantiateTemplateRequest<T>();
                requestType = request.GetType();
                _requestTypes.Add(requestKey, requestType);
            }
            request.Variables = contract;

            Type responseType;
            if (!_responseTypes.TryGetValue(responseKey, out responseType))
            {
                responseType = ReflectionUtils.BuildDynamicResponseType<T>();
                _responseTypes.Add(responseKey, responseType);
            }


            var callRpcMethod = typeof(TemplaterService).GetMethod(nameof(CallRpc), BindingFlags.Instance | BindingFlags.NonPublic);
            var callRpcGeneric = callRpcMethod.MakeGenericMethod(requestType, responseType, contract.GetType());
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