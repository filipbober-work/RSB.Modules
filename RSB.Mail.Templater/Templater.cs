using System;
using System.IO;
using System.Threading.Tasks;
using NLog;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace RSB.Mail.Templater
{
    public class Templater
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly MailSenderSettings _settings;

        public Templater(MailSenderSettings settings)
        {
            _settings = settings;

            var config = new TemplateServiceConfiguration
            {
                DisableTempFileLocking = true,
                CachingProvider = new DefaultCachingProvider(t => { }),
            };

            RazorEngineService.Create(config);
        }

        public void AddTemplate<T>() //where T : Models.ITemplate
        {
            AddTemplateAndCompile<T>(_settings.TemplatesPath);
        }

        //public async Task SendCreatedTemplateAsync(Models.ITemplate template)
        public async Task SendCreatedTemplateAsync(object contract)
        {
            Logger.Debug("Creating template body");
            string body = CreateTemplateBody(contract);
            Logger.Debug("Sending template");
            //await SendSmtpEmailAsync(message);
            await SendTemplateViaRabbit(body);

            Logger.Debug("Template sent");
        }

        public async Task SendTemplateViaRabbit(string body)
        {
            // TODO: Send template via Rabbit

            //var
        }

        //public async Task SendEmailAsync(IMailMessage message)
        //{
        //    Logger.Debug("Creating message body");
        //    message.Properties.Body = CreateTemplateBody(message);
        //    Logger.Debug("Sending email");
        //    await SendSmtpEmailAsync(message);

        //    Logger.Debug("Email sent");
        //}

        //private async Task SendSmtpEmailAsync(IMailMessage email)
        //{
        //    var message = new MimeMessage();

        //    foreach (var recipient in email.Properties.Recipients)
        //    {
        //        message.From.Add(new MailboxAddress(email.Properties.FromName, email.Properties.FromMail));
        //        message.To.Add(new MailboxAddress(recipient.ToName, recipient.ToMail));
        //        message.Subject = email.Properties.Subject;

        //        var builder = new BodyBuilder();
        //        builder.HtmlBody = string.Format(email.Properties.Body);
        //        message.Body = builder.ToMessageBody();

        //        using (var client = new SmtpClient())
        //        {
        //            await client.ConnectAsync(_settings.Hostname, _settings.Port);
        //            await client.AuthenticateAsync(_settings.Username, _settings.Password);
        //            await client.SendAsync(message);
        //            await client.DisconnectAsync(true);
        //        }
        //    }

        //}

        ////private static string CreateTemplateBody<T>(T template) where T : Models.ITemplate
        //public string CreateTemplateBody<T>(T template) //where T : Models.ITemplate
        //{
        //    var typeName = template.GetType().Name;
        //    return Engine.Razor.RunCompile(typeName, template.GetType(), template);
        //}

        //private static string CreateTemplateBody<T>(T template) where T : Models.ITemplate
        //private static string CreateTemplateBody<T>(T contract) //where T : Models.ITemplate
        //{
        //    //var typeName = template.GetType().Name;
        //    var typeName = contract.GetType().Name;
        //    return Engine.Razor.RunCompile(typeName, contract.GetType(), contract);
        //}

        public string CreateTemplateBody<T>(T contract) //where T : Models.ITemplate
        {
            //var typeName = template.GetType().Name;
            var typeName = contract.GetType().Name;
            return Engine.Razor.RunCompile(typeName, contract.GetType(), contract);
        }

        private static void AddTemplateAndCompile<T>(string templatesPath) //where T : Models.ITemplate
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
