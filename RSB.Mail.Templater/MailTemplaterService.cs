using System;
using NLog;
using RSB.Interfaces;
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

        public void Start()
        {
            Logger.Info("Starting {0}", nameof(MailTemplaterService));
            _mailManager = _container.GetInstance<MailManager>();

            _mailManager.Start();
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
