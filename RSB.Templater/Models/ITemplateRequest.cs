namespace RSB.Templater.Models
{
    public interface ITemplateRequest<T>
    {
        T Template { get; set; }
    }
}