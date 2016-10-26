using System;
using NLog;
using RSB.Mail.Templater.IoC;
using RSB.Transports.RabbitMQ.Settings;
using StructureMap;
using Topshelf;


namespace RSB.Mail.Templater
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main()
        {


            HostFactory.Run(x =>
            {
                x.SetServiceName("RSB.Modules.Mail.Templater");
                x.SetDisplayName("RSB.Modules.Mail.SmtpSender");
                x.SetDescription("This is mail templater service communicating by RSB.");

                x.StartAutomatically();

                x.UseNLog();

                x.Service<MailTemplaterService>(service =>
                {
                    service.ConstructUsing(srv => InitializeTemplaterService());

                    service.WhenStarted(srv => srv.Start());
                    service.WhenStopped(srv => srv.Stop());
                });
            });
        }

        private static MailTemplaterService InitializeTemplaterService()
        {
            Logger.Info("Initializing Templater Service...");

            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            var container = new Container(new TemplaterRegistry());
            return new MailTemplaterService(container);
        }

    }
}
