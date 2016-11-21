namespace RSB.Modules.Templater.Common.Contracts
{
    public interface ITemplateRequest<T>
    {
        T Variables { get; set; }
    }
}