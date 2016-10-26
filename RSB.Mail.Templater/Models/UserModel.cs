namespace RSB.Mail.Templater.Models
{
    public class UserModel : MailMessage
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsPremiumUser { get; set; }
    }
}
