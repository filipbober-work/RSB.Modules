using System;
using System.Threading.Tasks;
using NLog;
using RSB.Interfaces;
using RSB.Mail.Templater.Models;
using StructureMap;

namespace RSB.Mail.Templater
{
    class MailTemplaterService : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Container _container;

        private MailManager _mailManager;

        public MailTemplaterService(Container container)
        {
            _container = container;
        }

        public async Task Start()
        {
            Logger.Info("Starting {0}", nameof(MailTemplaterService));
            _mailManager = _container.GetInstance<MailManager>();
            var mailSender = _container.GetInstance<MailSender>();

            AddTemplates(mailSender);
            await _mailManager.Test();
        }

        public void Stop()
        {
            var bus = _container.GetInstance<IBus>();
            bus.Shutdown();
        }

        public void Dispose()
        {
            Logger.Info("Stopping {0}", nameof(MailTemplaterService));

            GC.SuppressFinalize(this);
        }

        private static void AddTemplates(MailSender mailSender)
        {
            mailSender.AddTemplate<SendUserRegisteredMail>();
        }

    }
}
