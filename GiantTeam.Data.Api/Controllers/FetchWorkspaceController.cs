using GiantTeam.ComponentModel;
using GiantTeam.WorkspaceAdministration.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static GiantTeam.WorkspaceAdministration.Services.FetchWorkspaceService;

namespace GiantTeam.Data.Api.Controllers;

public class FetchWorkspaceController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<FetchWorkspaceOutput> Post(
        [FromServices] FetchWorkspaceService fetchWorkspaceService,
        FetchWorkspaceInput input)
    {
        return await fetchWorkspaceService.FetchWorkspaceAsync(input);
    }
}
