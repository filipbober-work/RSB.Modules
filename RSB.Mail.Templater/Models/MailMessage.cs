namespace RSB.Mail.Templater.Models
{
    public class MailMessage
    {
        public string FromMail { get; set; }
        public string FromName { get; set; }
        public string ToMail { get; set; }
        public string ToName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
