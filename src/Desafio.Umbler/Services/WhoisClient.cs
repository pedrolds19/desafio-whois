using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Desafio.Umbler.Interfaces;

namespace Desafio.Umbler.Services
{
    public class WhoisClient : IWhoisClient
    {
        public async Task<string> GetDnsInformation(string domain)
        {
            try
            {
                var ipList = await Dns.GetHostAddressesAsync(domain);

                return ipList.FirstOrDefault()?.ToString() ?? "IP não encontrado";
            }
            catch
            {
                return "Não foi possível resolver o DNS";
            }
        }
        public async Task<string> QueryAsync(string domain)
        {
            try
            {
                var response = await Whois.NET.WhoisClient.QueryAsync(domain);
                return response.Raw; 
            }
            catch (Exception ex)
            {
                return $"Erro ao consultar Whois: {ex.Message}";
            }
        }
    }
}