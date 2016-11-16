using System;
using MailKit.Net.Smtp;
using MimeKit;
using NLog;
using RSB.Diagnostics;
using RSB.Modules.Mail.Contracts;
using RSB.Transports.RabbitMQ;
using RSB.Transports.RabbitMQ.Settings;

namespace RSB.Modules.Mail.SmtpSenderOld
{
    internal class MailSenderService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly RabbitMqTransportSettings _rabbitMqSettings;
        private readonly string _instanceName;
        private Bus _bus;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public MailSenderService(SmtpSettings smtpSettings, RabbitMqTransportSettings rabbitMqSettings, string instanceName)
        {
            _smtpSettings = smtpSettings;
            _rabbitMqSettings = rabbitMqSettings;
            _instanceName = instanceName;
        }

        public void Start()
        {
            _bus = new Bus(new RabbitMqTransport(_rabbitMqSettings));

            _bus.UseBusDiagnostics("MailSender", _instanceName);

            _bus.RegisterQueueHandler<MailMessage>(HandleMailMessage, _instanceName);
        }

        public void Stop()
        {
            _bus.Shutdown();
        }

        public void HandleMailMessage(MailMessage msg)
        {
            Logger.Info("Handling new mail message to {0}.", msg.ToMail);

            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(msg.FromName, msg.FromMail));
            message.To.Add(new MailboxAddress(msg.ToName, msg.ToMail));
            message.Subject = msg.Subject;

            message.Body = new TextPart("plain")
            {
                Text = msg.Body
            };

            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect(_smtpSettings.Hostname, _smtpSettings.Port, _smtpSettings.UseSsl);

                    if (!string.IsNullOrWhiteSpace(_smtpSettings.Username))
                    {
                        client.AuthenticationMechanisms.Remove("XOAUTH2");
                        client.Authenticate(_smtpSettings.Username, _smtpSettings.Password);
                    }

                    client.Send(message);

                    client.Disconnect(true);

                    Logger.Info("Message to {0} has been sent.", msg.ToMail);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while sending mail to {0}", msg.ToMail);
            }
        }
    }
}