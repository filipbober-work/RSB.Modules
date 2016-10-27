using System;
using System.IO;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using NLog;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RSB.Mail.Templater.Models;

namespace RSB.Mail.Templater
{
    class MailSender
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly MailSenderSettings _settings;
        //private readonly TemplateService _templateService;
        // TODO: Inject IRazorEngineService or use Engine.Razor? --- uzywac Engine.Razor
        private readonly IRazorEngineService _razor;

        public MailSender(MailSenderSettings settings/*, IRazorEngineService razor*/)
        {
            _settings = settings;
            //_razor = razor;
            // ---
            var config = new TemplateServiceConfiguration
            {
                DisableTempFileLocking = true,
                CachingProvider = new DefaultCachingProvider(t => { })
            };

            Engine.Razor = RazorEngineService.Create(config);
            _razor = Engine.Razor;
            // ---
        }

        public async Task Test()
        {
            var model = new UserModel
            {
                FromMail = _settings.HostAddress,
                FromName = _settings.Hostname,
                ToMail = "fxonus.mail@gmail.com",
                ToName = "Nameless One",
                Subject = "Return to sender",

                Name = "Nameless One",
                Email = "nameless@one.com",
                IsPremiumUser = false
            };

            // TODO: Not elegant - part of model is created based on model
            model.Body = CreateEmailBody(model);

            try
            {
                Logger.Debug("Creating message body");
                var body = CreateEmailBody(model);

                Logger.Debug("Preparing to send mail");
                await SendHtmlEmailAsync(body);
                await SendHtmlEmailAsync(model);

                Logger.Debug("Mail sent");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
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

        public async Task SendHtmlEmailAsync(MailMessage email)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("FxOnUs Mail From", email.FromMail));
            message.To.Add(new MailboxAddress("FxOnUs Mail To", email.ToMail));
            message.Subject = "Return to sender - MailMessage";

            var builder = new BodyBuilder();
            builder.HtmlBody = string.Format(email.Body);
            message.Body = builder.ToMessageBody();

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
            message.Subject = "Return to sender - htmlBody";

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
            var templateFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmailTemplates");
            // TODO: If path must be hardcoded maybe either make template names as their model.GetType.ToString()
            // or store model path as static string and register in TemplaterRegistry
            var templatePath = templateFolderPath + "\\WelcomeEmail.cshtml";

            // TODO: Ensure that key can be GetType.ToString - check for different MailMessage data
            //var key = mailMessage.GetType().ToString();
            var cacheName = mailMessage.GetType().ToString();
            var modelType = mailMessage.GetType();
            string emailHtmlBody;
            if (!_razor.IsTemplateCached(cacheName, modelType))
                emailHtmlBody = _razor.RunCompile(File.ReadAllText(templatePath), cacheName, null, mailMessage);
            else
                emailHtmlBody = _razor.Run(cacheName, modelType, mailMessage);

            // ---
            //_razor.AddTemplate(cacheName, WelcomeEmail);
            // ---

            //emailHtmlBody = _razor.RunCompile(File.ReadAllText(templatePath), templateName, null, model)
            //Engine.Razor.RunCompile(cache_name, typeof(MyModel) /* or model.GetType() or null for 'dynamic'*/, model)
            emailHtmlBody = _razor.RunCompile(File.ReadAllText(templatePath), cacheName, null, mailMessage);

            // TODO: Run once at startup (move away from here)
            _razor.AddTemplate(cacheName, File.ReadAllText(templatePath));
            _razor.RunCompile(cacheName, mailMessage.GetType(), mailMessage);

            return emailHtmlBody;
        }

    }
}
