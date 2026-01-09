using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Desafio.Umbler.Interfaces;
using Desafio.Umbler.Services;
using Desafio.Umbler.Models; 

namespace Desafio.Umbler.Controllers
{
    [Route("api/[controller]")]
    public class DomainController : Controller
    {
        private readonly IDomainService _domainService;

        public DomainController(IDomainService domainService)
        {
            _domainService = domainService;
        }

        [HttpGet("{domain}")]
        public async Task<IActionResult> Get(string domain)
        {
            var result = await _domainService.GetResultAsync(domain);
            if (result == null)
            {
                return BadRequest("Domínio inválido. Digite algo como 'umbler.com'");
            }
            return Ok(result);
        }
    }
}