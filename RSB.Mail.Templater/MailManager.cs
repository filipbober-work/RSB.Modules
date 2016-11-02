using System;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using RSB.Interfaces;
using RSB.Mail.Templater.Models;
using System.Reflection;

namespace RSB.Mail.Templater
{
    public class MailManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly MailSender _mailSender;
        private readonly IBus _bus;

        private bool _isInitialized;

        public MailManager(MailSender mailSender, IBus bus)
        {
            _mailSender = mailSender;
            _bus = bus;
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
            //var currentAssembly = GetType().GetTypeInfo().Assembly;

            var currentAssembly = Assembly.LoadFrom("EmailTemplates.dll");
            var implementedIMessage = currentAssembly.DefinedTypes.Where(type => type.ImplementedInterfaces.Any(inter => inter == typeof(IMailMessage))).ToList();

            var method = typeof(MailManager).GetMethod(nameof(RegisterTemplate), BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var t in implementedIMessage)
            {
                var generic = method.MakeGenericMethod(t);
                generic.Invoke(this, null);
            }
        }

        protected void RegisterTemplate<T>() where T : IMailMessage, new()
        {
            _mailSender.AddTemplate<T>();
            _bus.RegisterAsyncQueueHandler<T>(async msg => await SendEmailAsync(msg));
        }

        private async Task SendEmailAsync(IMailMessage message)
        {
            try
            {
                await _mailSender.SendEmailAsync(message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while sending message");
            }
        }
    }
}