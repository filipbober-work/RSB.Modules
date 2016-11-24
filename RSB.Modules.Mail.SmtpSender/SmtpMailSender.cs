using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using NLog;
using RSB.Modules.Mail.Contracts;

namespace RSB.Modules.Mail.SmtpSender
{
    public class SmtpMailSender : IMailSender
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly MailSenderSettings _settings;

        public SmtpMailSender(MailSenderSettings settings)
        {
            _settings = settings;
        }

        public async Task SendEmailAsync(SendMailMessage mail)
        {
            Logger.Debug("Sending email");
            var message = new MimeMessage();

            foreach (var recipient in mail.Recipients)
            {
                message.From.Add(new MailboxAddress(mail.FromName, mail.FromMail));
                message.To.Add(new MailboxAddress(recipient.ToName, recipient.ToMail));
                message.Subject = mail.Subject;

                var builder = new BodyBuilder();
                var regex = new Regex(@"src=""cid:(?<FirstNumber>[^""]*)");
                try
                {
                    foreach (Match match in regex.Matches(mail.Body))
                    {
                        var pathId = match.Groups["FirstNumber"].Value;
                        var image = builder.LinkedResources.Add(pathId);
                        image.ContentId = pathId;
                    }
                }
                catch (System.IO.IOException ex)
                {
                    Logger.Warn(ex, "Error while parsing image");
                }

                builder.HtmlBody = mail.Body;

                message.Body = builder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_settings.Hostname, _settings.Port, _settings.UseSsl);
                    await client.AuthenticateAsync(_settings.Username, _settings.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                Logger.Debug("Email sent");
            }

        }

    }
}