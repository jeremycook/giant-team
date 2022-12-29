using GiantTeam.Workspaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Data.Api.Controllers;

public class ImportDataController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<ImportDataOutput> Post(
        [FromServices] ImportDataService service,
        ImportDataInput input)
    {
        return await service.ImportDataAsync(input);
    }
}
