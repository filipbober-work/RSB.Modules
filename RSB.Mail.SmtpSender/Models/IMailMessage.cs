namespace RSB.Mail.SmtpSender.Models
{
    public interface IMailMessage
    {
        MailProperties Properties { get; set; }
    }
}