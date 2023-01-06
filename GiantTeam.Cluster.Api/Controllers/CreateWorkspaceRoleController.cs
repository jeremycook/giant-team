using GiantTeam.UserManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Cluster.Api.Controllers;

public class CreateWorkspaceRoleController : ControllerBase
{
    [HttpPost("/api/cluster/[Controller]")]
    public async Task<CreateWorkspaceRoleOutput> Post(
        [FromServices] CreateWorkspaceRoleService service,
        CreateWorkspaceRoleInput input)
    {
        return await service.CreateRoleAsync(input);
    }
}
