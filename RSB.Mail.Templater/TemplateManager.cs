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

        //private void InitializeTemplates()
        //{
        //    var tempaltesAssembly = Assembly.LoadFrom(_settings.TemplatesDll);
        //    var implementedIMessages = tempaltesAssembly.DefinedTypes.Where(type => type.ImplementedInterfaces.Any(inter => inter == typeof(ITemplate))).ToList();

        //    if (implementedIMessages.Count < 1)
        //    {
        //        Logger.Warn("No implementations of IMailMessage found in the given assembly");
        //    }

        //    var method = typeof(TemplateManager).GetMethod(nameof(RegisterTemplate), BindingFlags.Instance | BindingFlags.NonPublic);
        //    foreach (var t in implementedIMessages)
        //    {
        //        var generic = method.MakeGenericMethod(t);
        //        generic.Invoke(this, null);
        //    }
        //}

        private void InitializeTemplates()
        {
            var templatesAssembly = Assembly.LoadFrom(_settings.TemplatesDll);
            // TODO: Zaminiec inter == - znak rownosci na cos innego - moze jakas metode - wygooglowac

            //          bool isBar = foo.GetType().GetInterfaces().Any(x =>
            //x.IsGenericType &&
            //x.GetGenericTypeDefinition() == typeof(IBar<>));

            //var implementedIMessages = tempaltesAssembly.DefinedTypes.Where(type => type.ImplementedInterfaces.Any(inter => inter == typeof(ITemplateResponse<>))).ToList();

            var implementedResponses = templatesAssembly.DefinedTypes.Where(type => type.ImplementedInterfaces.Any(inter =>
                    inter.IsGenericType && inter.GetGenericTypeDefinition() == typeof(ITemplateResponse<>))).ToList();
            //inter == typeof(ITemplateResponse<>))).ToList();


            if (implementedResponses.Count < 1)
            {
                Logger.Warn("No implementations of IMailMessage found in the given assembly");
            }

            var method = typeof(TemplateManager).GetMethod(nameof(RegisterTemplate), BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var response in implementedResponses)
            {
                // ---
                // Find matching request
                var tmp = GetContractClassType(templatesAssembly, response);
                // ---

                var generic = method.MakeGenericMethod(response);
                generic.Invoke(this, null);


            }
        }

        private Type GetContractClassType(Assembly assembly, TypeInfo responseType)
        {
            var responseTypeStr = responseType.ToString();

            int pos = responseTypeStr.LastIndexOfAny(new[] { '.' });
            pos += 1;

            var TypeEndStr = "Response";
            var TypeStartStr = "Fill";

            //var result = rawtype.Substring(pos + 1 + 4);
            //result = result.Remove(result.Length - 8);

            var rawTypeStr = responseTypeStr.Substring(pos + TypeStartStr.Length);
            rawTypeStr = rawTypeStr.Remove(rawTypeStr.Length - TypeEndStr.Length);

            var responseNamespaceStr = responseTypeStr.Substring(0, pos);

            var fullRawTypeStr = responseNamespaceStr + rawTypeStr;

            //var implementedResponses = templatesAssembly.DefinedTypes.Where(type => type.ImplementedInterfaces.Any(inter =>
            //        inter.IsGenericType && inter.GetGenericTypeDefinition() == typeof(ITemplateResponse<>))).ToList();
            var rawType = assembly.DefinedTypes.Where(t => t.FullName == fullRawTypeStr);
            return rawType.FirstOrDefault();

            //return responseNamespaceStr + rawType;
        }

        //protected void RegisterTemplate<T>() where T : ITemplate, new()
        // TODO: Make private - read how
        protected void RegisterTemplate<TRequest, TResponse>()
        {
            //_templater.AddTemplate<T>();
            //_bus.RegisterAsyncQueueHandler<T>(async msg => await SendCreatedTemplateAsync(msg));

            // ---
            //_templater.AddTemplate<T>();
            //_bus.RegisterCallHandler<TRequest, TResponse>(
            // ---

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

        //private async Task SendEmailAsync(IMailMessage message)
        //{
        //    try
        //    {
        //        await _templater.SendCreatedTemplate(message);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error(ex, "Error while sending message");
        //    }
        //}
    }
}