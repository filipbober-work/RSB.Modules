using System;
using System.Linq;
using System.Reflection;
using NLog;
using RSB.Interfaces;
using RSB.Modules.Templater.Contracts;

namespace RSB.Modules.Templater
{
    public class TemplateManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Templater _templater;
        private readonly IBus _bus;
        private readonly TemplateManagerSettings _settings;

        private bool _isInitialized;

        public TemplateManager(Templater templater, IBus bus, TemplateManagerSettings settings)
        {
            _templater = templater;
            _bus = bus;
            _settings = settings;
        }

        public void Start()
        {
            if (_isInitialized)
                return;

            InitializeTemplates();

            _isInitialized = true;
        }

        private void InitializeTemplates()
        {
            var templatesAssembly = Assembly.LoadFrom(_settings.TemplatesDllPath);

            var implementedResponses = templatesAssembly.DefinedTypes.Where(type => type.ImplementedInterfaces.Any(inter =>
                    inter.IsGenericType && inter.GetGenericTypeDefinition() == typeof(ITemplateResponse<>))).ToList();

            if (implementedResponses.Count < 1)
            {
                Logger.Warn("No implementations of ITemplateResponse found in the given assembly");
            }

            var addTemplateMethod = typeof(Templater).GetMethod(nameof(_templater.AddTemplate), BindingFlags.Instance | BindingFlags.Public);

            foreach (var responseType in implementedResponses)
            {
                var contractType = GetContractClassType(templatesAssembly, responseType);
                Logger.Debug("Adding template template: {0}", contractType.Name);
                var addTemplateGeneric = addTemplateMethod.MakeGenericMethod(contractType);
                addTemplateGeneric.Invoke(_templater, null);

                var requestType = GetRequestClassType(templatesAssembly, responseType);
                var registerRpcMethod = typeof(TemplateManager).GetMethod(nameof(RegisterRpc), BindingFlags.Instance | BindingFlags.NonPublic);
                var registerRpcGeneric = registerRpcMethod.MakeGenericMethod(requestType, responseType, contractType);
                registerRpcGeneric.Invoke(this, null);
            }
        }

        private Type GetContractClassType(Assembly assembly, TypeInfo responseType)
        {
            var responseTypeStr = responseType.ToString();

            int pos = responseTypeStr.LastIndexOfAny(new[] { '.' });
            pos += 1;

            var typeEndStr = "Response";
            var typeStartStr = "Fill";

            var rawTypeStr = responseTypeStr.Substring(pos + typeStartStr.Length);
            rawTypeStr = rawTypeStr.Remove(rawTypeStr.Length - typeEndStr.Length);

            var responseNamespaceStr = responseTypeStr.Substring(0, pos);
            var fullRawTypeStr = responseNamespaceStr + rawTypeStr;
            var rawType = assembly.DefinedTypes.Where(t => t.FullName == fullRawTypeStr);

            return rawType.FirstOrDefault();
        }

        private Type GetRequestClassType(Assembly assembly, TypeInfo responseType)
        {
            var responseTypeStr = responseType.ToString();

            var typeEndStr = "Response";
            var requestTypeStr = responseTypeStr.Remove(responseTypeStr.Length - typeEndStr.Length);

            requestTypeStr += "Request";
            var requestType = assembly.DefinedTypes.Where(t => t.FullName == requestTypeStr);

            return requestType.FirstOrDefault();
        }

        private void RegisterRpc<TRequest, TResponse, T>()
            where TResponse : ITemplateResponse<T>, new()
            where TRequest : ITemplateRequest<T>, new()
        {
            _bus.RegisterCallHandler<TRequest, TResponse>(TemplateResponseHandler<TRequest, TResponse, T>, _settings.RoutingKey);

            Logger.Debug("Registered RPC for " + typeof(T));
        }

        private TResponse TemplateResponseHandler<TRequest, TResponse, T>(TRequest request)
            where TResponse : ITemplateResponse<T>, new()
            where TRequest : ITemplateRequest<T>
        {
            var response = new TResponse
            {
                Text = _templater.CreateTemplateBody(request.Template)
            };

            Logger.Debug("Response created for: {0}", typeof(T));

            return response;
        }

    }
}