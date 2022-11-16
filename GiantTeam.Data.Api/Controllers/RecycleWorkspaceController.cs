using GiantTeam.WorkspaceAdministration.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Data.Api.Controllers;

public class RecycleWorkspaceController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<RecycleWorkspaceService.RecycleWorkspaceOutput> Post(
        [FromServices] RecycleWorkspaceService recycleWorkspaceService,
        RecycleWorkspaceService.RecycleWorkspaceInput input)
    {
        return await recycleWorkspaceService.RecycleAsync(input);
    }
}
