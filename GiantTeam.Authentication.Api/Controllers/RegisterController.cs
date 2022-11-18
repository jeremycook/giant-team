using GiantTeam.UserManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GiantTeam.UserManagement.Services.JoinService;

namespace GiantTeam.Authentication.Api.Controllers;

[AllowAnonymous]
public class RegisterController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<JoinOutput> Post(
        [FromServices] JoinService joinService,
        JoinInput input) =>
        await joinService.JoinAsync(input);
}
