using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

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
            await SendEmailAsync();
        }

        public async Task SendEmailAsync()
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("FxOnUs Mail From", _settings.HostAddress));
            message.To.Add(new MailboxAddress("FxOnUs Mail To", "fxonus.mail@gmail.com"));
            message.Subject = "Return to sender";
            message.Body = new TextPart("plain") {Text = @"Returning to sender!"};

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
