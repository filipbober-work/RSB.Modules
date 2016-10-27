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

        public MailTemplaterService(Container container)
        {
            _container = container;
        }

        public async Task Start()
        {
            Logger.Info("Starting {0}", nameof(MailTemplaterService));
            var mailSender = _container.GetInstance<MailSender>();

            AddTemplates(mailSender);

            Logger.Debug("Sending message");
            try
            {
                var message = new SendUserRegisteredMail
                {
                    Properties = new MailProperties
                    {
                        FromMail = _settings.HostAddress,
                        FromName = _settings.Hostname,
                        Recipients = new System.Collections.Generic.List<Recipient>
                    {
                        new Recipient
                        {
                            ToMail = "fxonus.mail@gmail.com",
                            ToName = "Nameless One"
                        }
                    },
                        Subject = "Return to sender"
                    },

                    Name = "Nameless One",
                    Email = "nameless@one.com",
                    IsPremiumUser = false
                };

                await mailSender.SendEmailAsync(message);
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

        private static void AddTemplates(MailSender mailSender)
        {
            mailSender.AddTemplate<SendUserRegisteredMail>();
        }

    }
}
