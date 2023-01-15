using GiantTeam.Organization.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Organization.Api.Controllers;

[ApiController]
public class ImportDataController : ControllerBase
{
    [HttpPost("/api/organization/[Controller]")]
    public async Task<ImportDataOutput> Post(
        [FromServices] ImportDataService service,
        ImportDataInput input)
    {
        return await service.ImportDataAsync(input);
    }
}
