namespace RSB.Mail.Templater.Models
{
    public interface ITemplateRequest<T>
    {
        T Template { get; set; }
    }
}