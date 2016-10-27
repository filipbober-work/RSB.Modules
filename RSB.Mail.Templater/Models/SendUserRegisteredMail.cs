namespace RSB.Mail.Templater.Models
{
    public class SendUserRegisteredMail : IMailMessage
    {
        public MailProperties Properties { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsPremiumUser { get; set; }
    }
}
