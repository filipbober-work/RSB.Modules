using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using NLog;
using RSB.Mail.SmtpSender.Models;

namespace RSB.Mail.SmtpSender
{
    public class SmtpMailSender : IMailSender
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly MailSenderSettings _settings;

        public SmtpMailSender(MailSenderSettings settings)
        {
            _settings = settings;
        }

        public void SendEmail(SendMailMessage mail)
        {
            var message = new MimeMessage();

            foreach (var recipient in mail.Recipients)
            {
                message.From.Add(new MailboxAddress(mail.FromName, mail.FromMail));
                message.To.Add(new MailboxAddress(recipient.ToName, recipient.ToMail));
                message.Subject = mail.Subject;

                var builder = new BodyBuilder();
                builder.HtmlBody = string.Format(mail.Body);
                message.Body = builder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.Connect(_settings.Hostname, _settings.Port);
                    client.Authenticate(_settings.Username, _settings.Password);
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
        }

        public async Task SendEmailAsync(SendMailMessage mail)
        {
            var message = new MimeMessage();

            foreach (var recipient in mail.Recipients)
            {
                message.From.Add(new MailboxAddress(mail.FromName, mail.FromMail));
                message.To.Add(new MailboxAddress(recipient.ToName, recipient.ToMail));
                message.Subject = mail.Subject;

                var builder = new BodyBuilder();
                builder.HtmlBody = string.Format(mail.Body);
                message.Body = builder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_settings.Hostname, _settings.Port);
                    await client.AuthenticateAsync(_settings.Username, _settings.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }

        }
    }
}