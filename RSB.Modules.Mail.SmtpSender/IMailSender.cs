using System.Threading.Tasks;
using RSB.Modules.Mail.SmtpSender.Models;

namespace RSB.Modules.Mail.SmtpSender
{
    public interface IMailSender
    {
        Task SendEmailAsync(SendMailMessage mail);
    }
}