using System.Threading.Tasks;

namespace RSB.Modules.Templater.Common
{
    public interface ITemplaterService
    {
        Task<string> FillTemplateAsync<T>(T contract) where T : new();
    }
}