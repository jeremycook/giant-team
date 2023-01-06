using GiantTeam.Organization.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Organization.Api.Controllers;

public class AlterDatabaseController : ControllerBase
{
    [HttpPost("/api/organization/[Controller]")]
    public async Task<AlterDatabaseOutput> Post(
        [FromServices] AlterDatabaseService service,
        AlterDatabaseInput input)
    {
        return await service.ChangeDatabaseAsync(input);
    }
}
