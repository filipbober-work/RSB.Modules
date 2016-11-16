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

        private async Task SendEmailAsync(IMailMessage message)
        {

        }

    }
}