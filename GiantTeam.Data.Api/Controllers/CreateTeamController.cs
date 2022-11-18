using GiantTeam.UserManagement.Services;
using Microsoft.AspNetCore.Mvc;
using static GiantTeam.UserManagement.Services.CreateTeamService;

namespace GiantTeam.Data.Api.Controllers;

public class CreateTeamController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<CreateTeamOutput> Post(
        [FromServices] CreateTeamService service,
        CreateTeamInput input) =>
        await service.CreateTeamAsync(input);
}
