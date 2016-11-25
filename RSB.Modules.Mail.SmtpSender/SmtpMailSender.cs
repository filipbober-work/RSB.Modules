using System;
using System.IO;
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

            foreach (var recipient in mail.Recipients)
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(mail.FromName, mail.FromMail));
                message.To.Add(new MailboxAddress(recipient.ToName, recipient.ToMail));
                message.Subject = mail.Subject;

                var builder = new BodyBuilder();
                var mailBody = mail.Body;

                var regexImgFromFile = new Regex(@"src=""cid:(?<FileName>[^,|""]*)""");
                try
                {
                    foreach (Match match in regexImgFromFile.Matches(mailBody))
                    {
                        var pathId = match.Groups["FileName"].Value;

                        var image = builder.LinkedResources.Add(pathId);
                        image.ContentId = pathId;
                    }
                }
                catch (IOException ex)
                {
                    Logger.Warn(ex, "Error while parsing image");
                }

                var regexImgFromTemplate = new Regex(@"src=""cid:(?<Cid>\w+)(?<Remove>,data:.*;base64,)(?<Image>[^""]*)");
                try
                {
                    foreach (Match match in regexImgFromTemplate.Matches(mailBody))
                    {
                        var cid = match.Groups["Cid"].Value;
                        var data = Convert.FromBase64String(match.Groups["Image"].Value);
                        var ms = new MemoryStream(data);

                        mailBody = mailBody.Replace(match.Groups["Remove"].Value + match.Groups["Image"], "");

                        var image = builder.LinkedResources.Add("tmp", ms);
                        image.ContentId = cid;
                    }
                }
                catch (IOException ex)
                {
                    Logger.Warn(ex, "Error while parsing image");
                }

                builder.HtmlBody = mailBody;
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