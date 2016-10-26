using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using MimeKit;

namespace RSB.Mail.Templater
{
    class MailSender
    {

        public MailSender()
        {
            //fxonus.mail@gmail.com
        }

        public void Test()
        {
            SendEmail();
        }

        //public async Task SendEmailAsync(string email, string subject, string message)
        public void SendEmail()
        {
            //var emailMessage = new MimeMessage();

            //emailMessage.From.Add(new MailboxAddress("Nameless One", "nameless@example.com"));
            //emailMessage.To.Add(new MailboxAddress("", email));
            //emailMessage.Subject = subject;
            //emailMessage.Body = new TextPart("plain") {Text = message};

            //using (var client = new SmtpClient())
            //{
            //    client.LocalDomain = "some.domain.com";
            //    await client.ConnectAsync("smtp.relay.uri", 25, MailKit.Security.SecureSocketOptions.None).ConfigureAwait(false);
            //    await client.SendAsync(emailMessage).ConfigureAwait(false);
            //    await client.DisconnectAsync(true).ConfigureAwait(false);
            //}

            //using (var client = new Pop3Client())
            //{
            //    var Server = "gmail.com";
            //    var Port = "995";
            //    var UseSsl = false;
            //    var credentials = new NetworkCredential("fxonus.mail@gmail.com", "abc");
            //    var cancel = new CancellationTokenSource();
            //    var uri = new Uri(string.Format("pop{0}://{1}:{2}", (UseSsl ? "s" : ""), Server, Port));

            //    client.Connect(uri, cancel.Token);
            //    client.AuthenticationMechanisms.Remove("XOAUTH2");
            //    client.Authenticate(credentials, cancel.Token);

            //}

            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("FxOnUs Mail From", "fxonus.mail@gmail.com"));
            message.To.Add(new MailboxAddress("FxOnUs Mail To", "fxonus.mail@gmail.com"));
            message.Subject = "Return to sender";
            message.Body = new TextPart("plain") {Text = @"Returning to sender!"};

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587);

                client.Authenticate("fxonus.mail@gmail.com", "");

                client.Send(message);
                client.Disconnect(true);
            }


        }

    }
}
