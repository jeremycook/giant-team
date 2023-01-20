using GiantTeam.Organization.Etc.Models;
using GiantTeam.Organization.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Organization.Api.Controllers;

[ApiController]
public class CreateFolderController : ControllerBase
{
    [HttpPost("/api/organization/[Controller]")]
    public async Task<Inode> Post(
        [FromServices] CreateFolderService service,
        CreateFolderInput input)
    {
        return await service.CreateFolderAsync(input);
    }
}