using GiantTeam.Workspaces.Models;
using GiantTeam.Workspaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Data.Api.Controllers;

public class FetchWorkspaceController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<Workspace> Post(
        [FromServices] FetchWorkspaceService fetchWorkspaceService,
        FetchWorkspaceInput input)
    {
        return await fetchWorkspaceService.FetchWorkspaceAsync(input);
    }
}
