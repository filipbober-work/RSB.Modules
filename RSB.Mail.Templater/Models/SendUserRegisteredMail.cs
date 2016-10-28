using System;

namespace RSB.Mail.Templater.Models
{
    public class MailAttribute : Attribute
    {
    }

    [Mail]
    public class SendUserRegisteredMail : IMailMessage
    {
        public MailProperties Properties { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsPremiumUser { get; set; }
    }
}
