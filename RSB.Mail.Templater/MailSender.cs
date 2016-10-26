using System;
using System.IO;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using RazorEngine.Templating;
using RSB.Mail.Templater.Models;

namespace RSB.Mail.Templater
{
    class MailSender
    {
        private readonly MailSenderSettings _settings;

        public MailSender(MailSenderSettings settings)
        {
            _settings = settings;
        }

        public async Task Test()
        {
            var model = new UserModel
            {
                Name = "Nameless One",
                Email = "nameless@one.com",
                IsPremiumUser = false
            };

            var body = CreateEmailBody(model);

            await SendHtmlEmailAsync(body);
        }

        public async Task SendEmailAsync()
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("FxOnUs Mail From", _settings.HostAddress));
            message.To.Add(new MailboxAddress("FxOnUs Mail To", "fxonus.mail@gmail.com"));
            message.Subject = "Return to sender";
            message.Body = new TextPart("plain") { Text = @"Returning to sender!" };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_settings.Hostname, _settings.Port);
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

            }

        }

        public async Task SendHtmlEmailAsync(string htmlBody)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("FxOnUs Mail From", _settings.HostAddress));
            message.To.Add(new MailboxAddress("FxOnUs Mail To", "fxonus.mail@gmail.com"));
            message.Subject = "Return to sender";

            var builder = new BodyBuilder();
            builder.HtmlBody = string.Format(htmlBody);
            message.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_settings.Hostname, _settings.Port);
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

            }

        }

        private string CreateEmailBody<T>(T mailMessage) where T : MailMessage
        {
            // TODO: Inject path
            // TODO: Inject TemplateService as cache is per instance (use Singleton)
            var templateFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmailTemplates");
            var templateFilePath = templateFolderPath + "\\WelcomeEmail.cshtml";
            var templateService = new TemplateService();
            var emailHtmlBody = templateService.Parse(File.ReadAllText(templateFilePath), mailMessage, null, mailMessage.GetType().ToString());

            return emailHtmlBody;
        }

    }
}
