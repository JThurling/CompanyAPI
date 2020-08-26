using System.Threading.Tasks;
using Contracts;
using Microsoft.AspNetCore.Mvc;

namespace CompanyEmployees.Controllers
{
    [Route("api/companies")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v2")]
    public class CompaniesV2Controller : ControllerBase
    {
        private readonly IRepositoryManager _repositoryManager;

        public CompaniesV2Controller(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _repositoryManager.Company.GetAllCompaniesAsync(false);

            return Ok(companies);
        }
    }
}
