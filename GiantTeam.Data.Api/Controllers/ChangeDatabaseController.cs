using GiantTeam.Workspaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Data.Api.Controllers;

public class ChangeDatabaseController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<ChangeDatabaseOutput> Post(
        [FromServices] ChangeDatabaseService service,
        ChangeDatabaseInput input)
    {
        return await service.ChangeDatabaseAsync(input);
    }
}
