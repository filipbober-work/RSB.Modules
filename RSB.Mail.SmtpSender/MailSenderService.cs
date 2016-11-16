using System;
using NLog;
using RSB.Interfaces;
using StructureMap;

namespace RSB.Mail.SmtpSender
{
    public class MailSenderService : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Container _container;

        private MailManager _mailManager;

        public MailSenderService(Container container)
        {
            _container = container;
        }

        public void Start()
        {
            Logger.Info("Starting {0}", nameof(MailSenderService));

            _mailManager = _container.GetInstance<MailManager>();
        }

        public void Stop()
        {
            var bus = _container.GetInstance<IBus>();
            bus.Shutdown();
        }

        public void Dispose()
        {
            Logger.Info("Stopping {0}", nameof(MailSenderService));

            GC.SuppressFinalize(this);
        }

    }
}
