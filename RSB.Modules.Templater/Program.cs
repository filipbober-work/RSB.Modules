using System;
using NLog;
using RSB.Modules.Templater.IoC;
using StructureMap;
using Topshelf;

namespace RSB.Modules.Templater
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static void Main()
        {
            HostFactory.Run(x =>
            {
                x.SetServiceName("RSB.Modules.Templater");
                x.SetDisplayName("RSB.Modules.SmtpSender");
                x.SetDescription("This is templater service communicating by RSB.");

                x.StartAutomatically();

                x.UseNLog();

                x.Service<TemplaterService>(service =>
                {
                    service.ConstructUsing(srv => InitializeTemplaterService());

                    service.WhenStarted(srv => srv.Start());
                    service.WhenStopped(srv => srv.Stop());
                });
            });
        }

        private static TemplaterService InitializeTemplaterService()
        {
            Logger.Info("Initializing Templater Service...");

            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            var container = new Container(new TemplaterRegistry());
            return new TemplaterService(container);
        }

    }
}
