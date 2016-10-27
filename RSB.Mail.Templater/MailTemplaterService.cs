using System;
using System.Threading.Tasks;
using NLog;
using RSB.Interfaces;
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

        public async Task Start()
        {
            Logger.Info("Starting {0}", nameof(MailTemplaterService));
            var mailSender = _container.GetInstance<MailSender>();

            Logger.Debug("Sending message");
            try
            {
                await mailSender.Test();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while sending message");
            }

            Logger.Debug("Message sent");
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

    }
}
