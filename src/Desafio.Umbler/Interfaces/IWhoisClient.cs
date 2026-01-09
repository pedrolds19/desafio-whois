using System.Threading.Tasks;

namespace Desafio.Umbler.Interfaces
{
    public interface IWhoisClient
    {
        Task<string> QueryAsync(string domain);
        Task<string> GetDnsInformation(string domain);
    }
}