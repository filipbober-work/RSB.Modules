﻿using System;
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

        public MailSender(MailSenderSettings settings)
        {
            _settings = settings;

            var config = new TemplateServiceConfiguration
            {
                DisableTempFileLocking = true,
                CachingProvider = new DefaultCachingProvider(t => { }),
            };

            RazorEngineService.Create(config);

            AddTemplateAndCompile<UserModel>(_settings.TemplatesPath);

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

            Logger.Debug("Creating message body");
            model.Body = CreateEmailBody(model);

            Logger.Debug("Preparing to send mail");
            await SendHtmlEmailAsync(model);

            Logger.Debug("Mail sent");
        }

        public async Task SendHtmlEmailAsync(MailMessage email)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(email.FromName, email.FromMail));
            message.To.Add(new MailboxAddress(email.ToName, email.ToMail));
            message.Subject = email.Subject;

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

        private string CreateEmailBody<T>(T mailMessage) where T : MailMessage
        {
            var typeName = typeof(T).Name;
            return Engine.Razor.RunCompile(typeName, mailMessage.GetType(), mailMessage);
        }

        private void AddTemplateAndCompile<T>(string templatesPath) where T : MailMessage
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
