namespace RSB.Mail.SmtpSender
{
    public class MailSenderSettings
    {
        public string Hostname { get; set; }
        public string Address { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
    }
}