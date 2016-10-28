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
        private readonly IBus _bus;

        private bool _isInitialized;

        public MailManager(MailManagerSettings settings, MailSender mailSender, IBus bus)
        {
            _settings = settings;
            _mailSender = mailSender;
            _bus = bus;
        }

        public void Start()
        {
            if (_isInitialized)
                return;

            InitializeTemplates();

            _isInitialized = true;
        }

        public async Task TestSendMail()
        {
            var message = CreateMessage();

            Logger.Debug("Sending message");
            await SendEmailAsync(message);
            Logger.Debug("Message sent");
        }

        private void InitializeTemplates()
        {
            RegisterTemplate<SendUserRegisteredMail>();
        }

        private void RegisterTemplate<T>() where T : IMailMessage, new()
        {
            _mailSender.AddTemplate<T>();
            _bus.RegisterAsyncQueueHandler<T>(msg => SendEmailAsync(msg));
        }

        private async Task SendEmailAsync(IMailMessage message)
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