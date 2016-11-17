using System;
using NLog;
using RSB.Modules.Mail.SmtpSender.IoC;
using StructureMap;
using Topshelf;

namespace RSB.Modules.Mail.SmtpSender
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main()
        {
            HostFactory.Run(x =>
            {
                x.SetServiceName("RSB.Modules.MailSender");
                x.SetDisplayName("RSB.Modules.MailSender");
                x.SetDescription("This is mail sender service communicating by RSB.");

                x.StartAutomatically();

                x.UseNLog();

                x.Service<MailSenderService>(service =>
                {
                    service.ConstructUsing(srv => InitializeMailSenderService());

                    service.WhenStarted(srv => srv.Start());
                    service.WhenStopped(srv => srv.Stop());
                });
            });
        }

        private static MailSenderService InitializeMailSenderService()
        {
            Logger.Info("Initializing Mail Sender Service...");

            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            var container = new Container(new MailSenderRegistry());
            return new MailSenderService(container);
        }

    }
}
