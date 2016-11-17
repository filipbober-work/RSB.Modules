namespace RSB.Modules.Templater.Models
{
    public interface ITemplateRequest<T>
    {
        T Template { get; set; }
    }
}