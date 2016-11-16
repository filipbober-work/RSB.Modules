using System.Threading.Tasks;
using RSB.Mail.SmtpSender.Models;

namespace RSB.Mail.SmtpSender
{
    public interface IMailSender
    {
        Task SendEmailAsync(SendMailMessage mail);
    }
}