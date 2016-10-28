using System;
using System.Threading.Tasks;
using NLog;
using RSB.Interfaces;
using RSB.Mail.Templater.Models;

namespace RSB.Mail.Templater
{
    public class MailManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly MailManagerSettings _settings;
        private readonly MailSender _mailSender;
        private IBus _bus;

        public MailManager(MailManagerSettings settings, MailSender mailSender, IBus bus)
        {
            _settings = settings;
            _mailSender = mailSender;
            _bus = bus;
        }

        public async Task Test()
        {
            var message = CreateMessage();

            Logger.Debug("Sending message");
            await SendEmail(message);
            Logger.Debug("Message sent");
        }

        private async Task SendEmail(IMailMessage message)
        {
            try
            {
                await _mailSender.SendEmailAsync(message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while sending message");
            }
        }

        private IMailMessage CreateMessage()
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

            return message;
        }

    }
}