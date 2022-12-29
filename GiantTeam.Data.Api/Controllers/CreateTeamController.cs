using GiantTeam.UserManagement.Services;
using Microsoft.AspNetCore.Mvc;
using static GiantTeam.UserManagement.Services.CreateWorkspaceRoleService;

namespace GiantTeam.Data.Api.Controllers;

public class CreateTeamController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<CreateRoleOutput> Post(
        [FromServices] CreateWorkspaceRoleService service,
        CreateRoleInput input) =>
        await service.CreateRoleAsync(input);
}
