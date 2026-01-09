using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Desafio.Umbler.Interfaces;
using Desafio.Umbler.Models;
using Desafio.Umbler.ViewModels;
using System.Linq;

namespace Desafio.Umbler.Services
{
    public class DomainService : IDomainService
    {
        private readonly DatabaseContext _context;
        private readonly IWhoisClient _whoisClient;

        public DomainService(DatabaseContext context, IWhoisClient whoisClient)
        {
            _context = context;
            _whoisClient = whoisClient;
        }

        public async Task<DomainResponseViewModel> GetResultAsync(string domainName)
        {
            if (string.IsNullOrWhiteSpace(domainName) || !domainName.Contains(".") || domainName.Length < 3)
            {
                return null;
            }

            try
            {
                var domainEntity = await _context.Domains.FirstOrDefaultAsync(d => d.Name == domainName);
                
                if (domainEntity != null)
                {
                    var secondsSinceUpdate = (DateTime.Now - domainEntity.UpdatedAt).TotalSeconds;

                    if (secondsSinceUpdate < domainEntity.Ttl)
                    {
                        return MapToViewModel(domainEntity);
                    }
                }


                string ipData = "Não encontrado";
                string cleanNameServers = "Consultado via Whois";
                string hostedAt = "Não identificado";

                try
                {
                    ipData = await _whoisClient.GetDnsInformation(domainName);
                    if (string.IsNullOrWhiteSpace(ipData)) ipData = "IP Privado ou Não Encontrado";
                }
                catch { ipData = "Erro ao obter IP"; }

                string whoisRawData = "";
                try
                {
                    whoisRawData = await _whoisClient.QueryAsync(domainName);
                }
                catch { whoisRawData = ""; }

                if (!string.IsNullOrWhiteSpace(whoisRawData))
                {
                    try
                    {
                        var nameServersList = whoisRawData
                            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                            .Where(line => line.Trim().StartsWith("Name Server", StringComparison.OrdinalIgnoreCase) ||
                                           line.Trim().StartsWith("nserver", StringComparison.OrdinalIgnoreCase))
                            .Select(line => line.Split(':').Last().Trim())
                            .Distinct()
                            .ToList();

                        if (nameServersList.Any())
                        {
                            cleanNameServers = string.Join(", ", nameServersList);
                        }
                    }
                    catch {}

                    try
                    {
                        var registrarLine = whoisRawData
                            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                            .FirstOrDefault(line => line.Trim().StartsWith("Registrar:", StringComparison.OrdinalIgnoreCase) ||
                                                    line.Trim().StartsWith("Registrant Organization:", StringComparison.OrdinalIgnoreCase));

                        if (registrarLine != null)
                        {
                            hostedAt = registrarLine.Split(':')[1].Trim();
                        }
                    }
                    catch {  }
                }

                if (domainEntity == null)
                {
                    domainEntity = new Domain
                    {
                        Name = domainName,
                        Ip = ipData,
                        HostedAt = hostedAt, 
                        WhoIs = cleanNameServers,
                        Ttl = 600,
                        UpdatedAt = DateTime.Now
                    };
                    _context.Domains.Add(domainEntity);
                }
                else
                {
                    domainEntity.Ip = ipData;
                    domainEntity.HostedAt = hostedAt;
                    domainEntity.WhoIs = cleanNameServers;
                    domainEntity.UpdatedAt = DateTime.Now;
                    _context.Domains.Update(domainEntity);
                }

                await _context.SaveChangesAsync();
                return MapToViewModel(domainEntity);
            }
            catch (Exception)
            {
                return new DomainResponseViewModel
                {
                    Domain = domainName,
                    Ip = "-",
                    NameServers = "Erro ao consultar provedor",
                    HostedAt = "-"
                };
            }
        }

        private DomainResponseViewModel MapToViewModel(Domain domain)
        {
            return new DomainResponseViewModel
            {
                Domain = domain.Name,
                Ip = domain.Ip,
                NameServers = domain.WhoIs,
                HostedAt = domain.HostedAt
            };
        }
    }
}