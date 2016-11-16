using System;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using RSB.Interfaces;
using RSB.Mail.Templater.Models;
using System.Reflection;

namespace RSB.Mail.Templater
{
    public class TemplateManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Templater _templater;
        private readonly IBus _bus;
        private readonly MailManagerSettings _settings;

        private bool _isInitialized;

        public TemplateManager(Templater templater, IBus bus, MailManagerSettings settings)
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
            var templatesAssembly = Assembly.LoadFrom(_settings.TemplatesDll);

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

        // TODO: Make private
        protected void RegisterRpc<TRequest, TResponse, T>()
            where TResponse : ITemplateResponse<T>, new()
            where TRequest : ITemplateRequest<T>, new()
        {
            _bus.RegisterCallHandler<TRequest, TResponse>(TmpHandler<TRequest, TResponse, T>);
        }

        private TResponse TmpHandler<TRequest, TResponse, T>(TRequest request)
            where TResponse : ITemplateResponse<T>, new()
            where TRequest : ITemplateRequest<T>
        {
            var response = new TResponse();
            response.Text = _templater.CreateTemplateBody(request.Template);

            return response;
        }

        private async Task SendCreatedTemplateAsync(ITemplate message)
        {
            try
            {
                await _templater.SendCreatedTemplateAsync(message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while sending message");
            }
        }

    }
}