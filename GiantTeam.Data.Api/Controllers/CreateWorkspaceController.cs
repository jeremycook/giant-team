using GiantTeam.WorkspaceAdministration.Services;
using Microsoft.AspNetCore.Mvc;
using static GiantTeam.WorkspaceAdministration.Services.CreateWorkspaceService;

namespace GiantTeam.Data.Api.Controllers;

public class CreateWorkspaceController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<CreateWorkspaceOutput> Post(
        [FromServices] CreateWorkspaceService createWorkspaceService,
        CreateWorkspaceInput input) =>
        await createWorkspaceService.CreateWorkspaceAsync(input);
}