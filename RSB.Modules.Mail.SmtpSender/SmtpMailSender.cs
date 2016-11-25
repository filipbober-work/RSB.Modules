using System;
using System.IO;
using System.Net.Mime;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using NLog;
using RSB.Modules.Mail.Contracts;
using static System.Net.Mime.MediaTypeNames;

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



                //var matches = regexImgFromTemplate.Matches(mail.Body);
                ////var matches = regexImgFromFile.Matches(mail.Body);
                //if (matches.Count > 0)
                //{
                //    try
                //    {
                //        foreach (Match match in regexImgFromTemplate.Matches(mail.Body))
                //        //foreach (Match match in matches)
                //        {
                //            string cid = match.Groups["Cid"].Value;
                //            byte[] data = System.Convert.FromBase64String(match.Groups["Image"].Value);
                //            MemoryStream ms = new MemoryStream(data);

                //            // TODO: temp variable
                //            mail.Body = mail.Body.Replace(match.Groups["Remove"].Value + match.Groups["Image"], "");

                //            var tmp = Path.GetTempFileName();

                //            using (var stream = File.Open(tmp, FileMode.Open, FileAccess.ReadWrite))
                //            {
                //                stream.Write(data, 0, data.Length);
                //            }


                //            var image = builder.LinkedResources.Add(tmp, ms);
                //            image.ContentId = cid;
                //        }
                //    }
                //    catch (System.IO.IOException ex)
                //    {
                //        Logger.Warn(ex, "Error while parsing image");
                //    }
                //}
                //else
                //{
                //    matches = regexImgFromFile.Matches(mail.Body);
                //    try
                //    {
                //        foreach (Match match in regexImgFromFile.Matches(mail.Body))
                //        //foreach (Match match in matches)
                //        {
                //            var success = match.Success;
                //            var pathId = match.Groups["FileName"].Value;

                //            //string baseContent = "

                //            var image = builder.LinkedResources.Add(pathId);
                //            image.ContentId = pathId;
                //        }
                //    }
                //    catch (System.IO.IOException ex)
                //    {
                //        Logger.Warn(ex, "Error while parsing image");
                //    }
                //}

                string mailBody = mail.Body;

                //var regexImgFromFile = new Regex(@"src=""cid:(?<FileName>[^,""]*)");
                var regexImgFromFile = new Regex(@"src=""cid:(?<FileName>[^,|""]*)""");
                try
                {
                    foreach (Match match in regexImgFromFile.Matches(mailBody))
                    //foreach (Match match in matches)
                    {
                        var success = match.Success;
                        var pathId = match.Groups["FileName"].Value;

                        //string baseContent = "

                        var image = builder.LinkedResources.Add(pathId);
                        image.ContentId = pathId;
                    }
                }
                catch (System.IO.IOException ex)
                {
                    Logger.Warn(ex, "Error while parsing image");
                }



                var regexImgFromTemplate = new Regex(@"src=""cid:(?<Cid>\w+)(?<Remove>,data:.*;base64,)(?<Image>[^""]*)");
                try
                {
                    foreach (Match match in regexImgFromTemplate.Matches(mailBody))
                    //foreach (Match match in matches)
                    {
                        string cid = match.Groups["Cid"].Value;
                        byte[] data = System.Convert.FromBase64String(match.Groups["Image"].Value);
                        MemoryStream ms = new MemoryStream(data);

                        // TODO: temp variable
                        mailBody = mailBody.Replace(match.Groups["Remove"].Value + match.Groups["Image"], "");

                        //var tmp = Path.GetTempFileName();

                        //using (var stream = File.Open(tmp, FileMode.Open, FileAccess.ReadWrite))
                        //{
                        //    stream.Write(data, 0, data.Length);
                        //}
                        //var image = builder.LinkedResources.Add(tmp, ms);

                        var image = builder.LinkedResources.Add("tmp", ms);
                        image.ContentId = cid;
                    }
                }
                catch (System.IO.IOException ex)
                {
                    Logger.Warn(ex, "Error while parsing image");
                }






                //var builder2 = new BodyBuilder();
                //var regexImgFromTemplate = new Regex(@"src=""cid:(?<Cid>\w+)(?<Remove>,data:.*;base64,)(?<Image>[^""]*)");
                //try
                //{
                //    foreach (Match match in regexImgFromTemplate.Matches(mail.Body))
                //    {
                //        string cid = match.Groups["Cid"].Value;
                //        byte[] data = System.Convert.FromBase64String(match.Groups["Image"].Value);
                //        MemoryStream ms = new MemoryStream(data);

                //        // TODO: temp variable
                //        mail.Body = mail.Body.Replace(match.Groups["Remove"].Value + match.Groups["Image"], "");

                //        var tmp = Path.GetTempFileName();

                //        using (var stream = File.Open(tmp, FileMode.Open, FileAccess.ReadWrite))
                //        {
                //            stream.Write(data, 0, data.Length);
                //        }


                //        //var contentObject = new ContentObject(ms);
                //        var image = builder.LinkedResources.Add(tmp, ms);
                //        image.ContentId = cid;
                //        //var entity = new MimeEntity();

                //        //var pathId = match.Groups["FirstNumber"].Value;

                //        ////string baseContent = "

                //        //var image = builder2.LinkedResources.Add(pathId);
                //        //image.ContentId = pathId;
                //    }
                //}
                //catch (System.IO.IOException ex)
                //{
                //    Logger.Warn(ex, "Error while parsing image");
                //}




                // ---
                //var img2 = builder.LinkedResources.Add(
                //var attachment = new MimePart("image", "png")
                //{
                //    ContentObject = new ContentObject("images/img.png"), ContentEncoding.Base64
                //};
                // ---

                //http://stackoverflow.com/questions/31417916/mimekit-mimemessage-to-browser-renderable-html

                builder.HtmlBody = mailBody;
               // builder.TextBody = mail.Body;
                message.Body = builder.ToMessageBody();


                //var body = new TextPart("plain") { Text = mail.Body };
                //var multipart = new Multipart("mixed");
                //multipart.Add(body);

                //message.Body = multipart;

                var bd = message.Body;


                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_settings.Hostname, _settings.Port, _settings.UseSsl);
                    //client.ServerCertificateValidationCallback

                    //await client.ConnectAsync(_settings.Hostname, _settings.Port, MailKit.Security.SecureSocketOptions.Auto);
                    await client.AuthenticateAsync(_settings.Username, _settings.Password);
                    //await client.SendAsync(message);


                    //var format = new FormatOptions();
                    //format.International = true;
                    //await client.SendAsync(format, message);
                    //var contentType = message.Body.ContentType;
                    //client.
                    await client.SendAsync(message);

                    await client.DisconnectAsync(true);
                }

                Logger.Debug("Email sent");
            }

        }

    }
}