using System;
using System.Threading.Tasks;
using NLog;
using RSB.Mail.Templater.Models;

namespace RSB.Mail.Templater
{
    public class MailManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly MailManagerSettings _settings;
        private readonly MailSender _mailSender;

        public MailManager(MailManagerSettings settings, MailSender mailSender)
        {
            _settings = settings;
            _mailSender = mailSender;
        }

        public async Task Test()
        {

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

                await _mailSender.SendEmailAsync(message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while sending message");
            }
        }

    }
}