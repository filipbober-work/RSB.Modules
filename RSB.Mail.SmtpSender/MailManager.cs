using System;
using System.Threading.Tasks;
using NLog;
using RSB.Interfaces;
using RSB.Mail.SmtpSender.Models;

namespace RSB.Mail.SmtpSender
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
            // TODO: Try to make it async
            //_bus.RegisterQueueHandler<SendMailMessage>(HandleMailMessage, _settings.InstanceName);

            _bus.RegisterAsyncQueueHandler<SendMailMessage>(SendEmailAsync, _settings.InstanceName);
        }

        private void HandleMailMessage(SendMailMessage message)
        {
            SendEmail(message);
        }

        private void SendEmail(SendMailMessage message)
        {
            try
            {
                _mailSender.SendEmail(message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while sending message");
            }
        }

        private async Task SendEmailAsync(SendMailMessage message)
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