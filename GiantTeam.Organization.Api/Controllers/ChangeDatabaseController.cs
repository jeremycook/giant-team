using GiantTeam.Workspaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Organization.Api.Controllers;

public class ChangeDatabaseController : ControllerBase
{
    [HttpPost("/api/organization/[Controller]")]
    public async Task<ChangeDatabaseOutput> Post(
        [FromServices] ChangeDatabaseService service,
        ChangeDatabaseInput input)
    {
        return await service.ChangeDatabaseAsync(input);
    }
}
