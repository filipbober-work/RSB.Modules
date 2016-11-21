using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NLog;
using RSB.Interfaces;
using RSB.Modules.Templater.Common.Contracts;
using RSB.Modules.Templater.Common.Utils;

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

            var implementedTemplates = GetTypesWithAttribute<TemplateAttribute>(templatesAssembly).ToList();
            if (implementedTemplates.Count < 1)
            {
                Logger.Warn("No implementations with TemplateAttribute found in the given assembly");
            }

            var addTemplateMethod = typeof(Templater).GetMethod(nameof(_templater.AddTemplate), BindingFlags.Instance | BindingFlags.Public);

            foreach (var contract in implementedTemplates)
            {
                // ReSharper disable once PassStringInterpolation
                Logger.Debug("Adding template template: {0}", contract.Name);
                var addTemplateGeneric = addTemplateMethod.MakeGenericMethod(contract);
                addTemplateGeneric.Invoke(_templater, null);

                var buildTemplateRequestTypeMethod = typeof(ReflectionUtils).GetMethod(nameof(ReflectionUtils.BuildDynamicRequestType), BindingFlags.Static | BindingFlags.Public);
                var buildTemplateRequestGeneric = buildTemplateRequestTypeMethod.MakeGenericMethod(contract);
                var requestType = buildTemplateRequestGeneric.Invoke(this, null) as Type;

                var buildTemplateResponseTypeMethod = typeof(ReflectionUtils).GetMethod(nameof(ReflectionUtils.BuildDynamicResponseType), BindingFlags.Static | BindingFlags.Public);
                var buildTemplateResponseGeneric = buildTemplateResponseTypeMethod.MakeGenericMethod(contract);
                var responseType = buildTemplateResponseGeneric.Invoke(this, null) as Type;

                var registerRpcMethod = typeof(TemplateManager).GetMethod(nameof(RegisterRpc), BindingFlags.Instance | BindingFlags.NonPublic);
                var registerRpcGeneric = registerRpcMethod.MakeGenericMethod(requestType, responseType, contract);
                registerRpcGeneric.Invoke(this, null);
            }

        }

        private static IEnumerable<TypeInfo> GetTypesWithAttribute<T>(Assembly assembly)
        {
            foreach (var type in assembly.DefinedTypes)
            {
                if (type.GetCustomAttributes(typeof(T), true).Length > 0)
                {
                    yield return type;
                }
            }
        }

        private void RegisterRpc<TRequest, TResponse, T>()
            where TRequest : ITemplateRequest<T>, new()
            where TResponse : ITemplateResponse<T>, new()
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
                Text = _templater.CreateTemplateBody(request.Variables)
            };

            // ReSharper disable once PassStringInterpolation
            Logger.Debug("Response created for: {0}", typeof(T));

            return response;
        }

    }
}