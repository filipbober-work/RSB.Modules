using System;
using System.IO;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RSB.Mail.Templater.Models;

namespace RSB.Mail.Templater
{
    class MailSender
    {
        private readonly MailSenderSettings _settings;
        //private readonly TemplateService _templateService;
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
                Name = "Nameless One",
                Email = "nameless@one.com",
                IsPremiumUser = false
            };

            var model2 = new UserModel
            {
                Name = "Nameless One2",
                Email = "nameless@one.com2",
                IsPremiumUser = false
            };

            try
            {
                var body = CreateEmailBody(model);
                await SendHtmlEmailAsync(body);

                var body2 = CreateEmailBody(model2);
                await SendHtmlEmailAsync(body2);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error");
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
            //var config = new TemplateServiceConfiguration
            //{
            //    DisableTempFileLocking = true,
            //    CachingProvider = new DefaultCachingProvider(t => { })
            //};

            //Engine.Razor = RazorEngineService.Create(config);
            // Engine.Razor.AddTemplateAndCompile<UserModel>();
            //if (!Engine.Razor.IsTemplateCached(

            // TODO: Inject path
            var templateFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmailTemplates");
            var templatePath = templateFolderPath + "\\WelcomeEmail.cshtml";

            // TODO: Ensure that key can be GetType.ToString - check for different MailMessage data
            //_razor.RunCompile(templateFilePath, mailMessage.GetType(), mailMessage);

            //var key = templatePath;//mailMessage.GetType().ToString();
            var key = mailMessage.GetType().ToString();
            var modelType = mailMessage.GetType();
            string emailHtmlBody;
            if (!_razor.IsTemplateCached(key, modelType))
                //_razor.Compile(templatePath, key, modelType);
                //emailHtmlBody = _razor.RunCompile(templatePath, key, null, mailMessage);
                emailHtmlBody = _razor.RunCompile(File.ReadAllText(templatePath), key, null, mailMessage);
            else
                emailHtmlBody = _razor.Run(key, modelType, mailMessage);
            //Engine.Razor.Compile(templatePath, mailMessage.GetType().ToString(), mailMessage.GetType());


            //var emailHtmlBody = _templateService.Parse(File.ReadAllText(templatePath), mailMessage, null, mailMessage.GetType().ToString());

            return emailHtmlBody;
        }

    }
}
