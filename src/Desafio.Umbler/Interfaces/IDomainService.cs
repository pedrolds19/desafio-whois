using System.Threading.Tasks;
using Desafio.Umbler.ViewModels;

namespace Desafio.Umbler.Interfaces
{
    public interface IDomainService
    {
        Task<DomainResponseViewModel> GetResultAsync(string domain);
    }
}