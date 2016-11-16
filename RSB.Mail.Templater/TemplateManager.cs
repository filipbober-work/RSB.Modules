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
            var tempaltesAssembly = Assembly.LoadFrom(_settings.TemplatesDll);
            // TODO: Zaminiec inter == - znak rownosci na cos innego - moze jakas metode - wygooglowac

            //          bool isBar = foo.GetType().GetInterfaces().Any(x =>
            //x.IsGenericType &&
            //x.GetGenericTypeDefinition() == typeof(IBar<>));

            //var implementedIMessages = tempaltesAssembly.DefinedTypes.Where(type => type.ImplementedInterfaces.Any(inter => inter == typeof(ITemplateResponse<>))).ToList();

            var implementedIMessages = tempaltesAssembly.DefinedTypes.Where(type => type.ImplementedInterfaces.Any(inter =>
                    inter.IsGenericType && inter.GetGenericTypeDefinition() == typeof(ITemplateResponse<>))).ToList();
            //inter == typeof(ITemplateResponse<>))).ToList();


            if (implementedIMessages.Count < 1)
            {
                Logger.Warn("No implementations of IMailMessage found in the given assembly");
            }

            var method = typeof(TemplateManager).GetMethod(nameof(RegisterTemplate), BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var t in implementedIMessages)
            {
                var generic = method.MakeGenericMethod(t);
                generic.Invoke(this, null);










                var myMethod = t.GetMethods()
                                .Where(m => m.Name == "MyMethod")
                                .Select(m => new
                                {
                                    Method = m,
                                    Params = m.GetParameters(),
                                    Args = m.GetGenericArguments()
                                })
                                .Where(x => x.Params.Length == 1
                                            && x.Args.Length == 1
                                            && x.Params[0].ParameterType == x.Args[0])
                                .Select(x => x.Method)
                                .First();





            }
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