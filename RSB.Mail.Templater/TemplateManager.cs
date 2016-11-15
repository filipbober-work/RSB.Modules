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
            var tempaltesAssembly = Assembly.LoadFrom(_settings.TemplatesDll);
            var implementedIMessages = tempaltesAssembly.DefinedTypes.Where(type => type.ImplementedInterfaces.Any(inter => inter == typeof(ITemplate))).ToList();

            if (implementedIMessages.Count < 1)
            {
                Logger.Warn("No implementations of IMailMessage found in the given assembly");
            }

            var method = typeof(TemplateManager).GetMethod(nameof(RegisterTemplate), BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var t in implementedIMessages)
            {
                var generic = method.MakeGenericMethod(t);
                generic.Invoke(this, null);
            }
        }

        protected void RegisterTemplate<T>() where T : ITemplate, new()
        {
            _templater.AddTemplate<T>();
            _bus.RegisterAsyncQueueHandler<T>(async msg => await SendCreatedTemplateAsync(msg));
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