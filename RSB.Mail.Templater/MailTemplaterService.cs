using System;
using NLog;
using RSB.Interfaces;
using RSB.Transports.RabbitMQ.Settings;
using StructureMap;

namespace RSB.Mail.Templater
{
    class MailTemplaterService : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Container _container;

        public MailTemplaterService(Container container)
        {
            _container = container;
        }

        public void Start()
        {
            Logger.Info("Starting {0}", nameof(MailTemplaterService));

            // ---
            Logger.Debug("Senging message");
            var mailSender = new MailSender();
            mailSender.Test();
            Logger.Debug("Message sent");
            // ---
        }

        public void Stop()
        {
            var bus = _container.GetInstance<IBus>();
            bus.Shutdown();
        }

        public void Dispose()
        {
            Logger.Info("Stopping {0}", nameof(MailTemplaterService));

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free managed resources
                // if != null -> dispose -> set to null
            }
        }
    }
}
