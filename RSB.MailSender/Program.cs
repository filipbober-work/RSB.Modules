using System;
using NLog;
using RSB.MailSender.IoC;
using StructureMap;
using Topshelf;

namespace RSB.MailSender
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.SetServiceName("RSB.Modules.Mail.Templater");
                x.SetDisplayName("RSB.Modules.Mail.SmtpSender");
                x.SetDescription("This is mail templater service communicating by RSB.");

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
