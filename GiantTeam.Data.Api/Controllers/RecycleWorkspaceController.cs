using GiantTeam.WorkspaceInteraction.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Data.Api.Controllers;

public class RecycleWorkspaceController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<RecycleWorkspaceService.RecycleWorkspaceOutput> Post(
        [FromServices] RecycleWorkspaceService deleteWorkspaceService,
        RecycleWorkspaceService.RecycleWorkspaceInput input)
    {
        return await deleteWorkspaceService.RecycleAsync(input);
    }
}
