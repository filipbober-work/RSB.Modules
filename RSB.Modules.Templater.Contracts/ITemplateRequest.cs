namespace RSB.Modules.Templater.Contracts
{
    public interface ITemplateRequest<T>
    {
        T Template { get; set; }
    }
}