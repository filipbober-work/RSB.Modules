using System;
using System.Threading.Tasks;
using NLog;
using RSB.Interfaces;
using RSB.Modules.Mail.SmtpSender.Models;

namespace RSB.Modules.Mail.SmtpSender
{
    public class MailManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IMailSender _mailSender;
        private readonly IBus _bus;
        private readonly MailManagerSettings _settings;

        public MailManager(IMailSender mailSender, IBus bus, MailManagerSettings settings)
        {
            _mailSender = mailSender;
            _bus = bus;
            _settings = settings;
        }

        public void Start()
        {
            _bus.RegisterAsyncQueueHandler<SendMailMessage>(HandleMailMessageAsync, _settings.RabbitRoutingKey);
        }

        private async Task HandleMailMessageAsync(SendMailMessage message)
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

    }
}