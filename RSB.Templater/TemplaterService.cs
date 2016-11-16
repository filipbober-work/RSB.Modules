using System;
using NLog;
using RSB.Interfaces;
using StructureMap;

namespace RSB.Templater
{
    class TemplaterService : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Container _container;

        private TemplateManager _templateManager;

        public TemplaterService(Container container)
        {
            _container = container;
        }

        public void Start()
        {
            Logger.Info("Starting {0}", nameof(TemplaterService));
            _templateManager = _container.GetInstance<TemplateManager>();

            _templateManager.Start();
        }

        public void Stop()
        {
            var bus = _container.GetInstance<IBus>();
            bus.Shutdown();
        }

        public void Dispose()
        {
            Logger.Info("Stopping {0}", nameof(TemplaterService));

            GC.SuppressFinalize(this);
        }

    }
}
