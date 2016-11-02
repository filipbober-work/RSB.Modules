using System;
using System.IO;
using System.Security;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web.Razor;
using MailKit.Net.Smtp;
using MimeKit;
using NLog;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RSB.Mail.Templater.Models;

namespace RSB.Mail.Templater
{
    public class MailSender
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly MailSenderSettings _settings;

        public MailSender(MailSenderSettings settings)
        {
            _settings = settings;

            var config = new TemplateServiceConfiguration
            {
                DisableTempFileLocking = true,
                CachingProvider = new DefaultCachingProvider(t => { }),
            };

            RazorEngineService.Create(config);
        }

        public void AddTemplate<T>() where T : IMailMessage
        {
            AddTemplateAndCompile<T>(_settings.TemplatesPath);
        }

        public async Task SendEmailAsync(IMailMessage message)
        {
            Logger.Debug("Creating message body");
            message.Properties.Body = CreateEmailBody(message);
            Logger.Debug("Sending email");
            await SendSmtpEmailAsync(message);

            Logger.Debug("Email sent");
        }

        private async Task SendSmtpEmailAsync(IMailMessage email)
        {
            var message = new MimeMessage();

            foreach (var recipient in email.Properties.Recipients)
            {
                message.From.Add(new MailboxAddress(email.Properties.FromName, email.Properties.FromMail));
                message.To.Add(new MailboxAddress(recipient.ToName, recipient.ToMail));
                message.Subject = email.Properties.Subject;

                var builder = new BodyBuilder();
                builder.HtmlBody = string.Format(email.Properties.Body);
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

        private static string CreateEmailBody<T>(T mailMessage) where T : IMailMessage
        {
            var typeName = mailMessage.GetType().Name;
            return Engine.Razor.RunCompile(typeName, mailMessage.GetType(), mailMessage);
        }

        private static void AddTemplateAndCompile<T>(string templatesPath) where T : IMailMessage
        {
            var typeName = typeof(T).Name;
            var templatePath = Path.Combine(templatesPath, typeName) + ".cshtml";

            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"Cannot find file {templatePath}.");

            Engine.Razor.AddTemplate(typeName, File.ReadAllText(templatePath));
            Engine.Razor.Compile(typeName, typeof(T));
        }

    }
}
