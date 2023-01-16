using GiantTeam.Organization.Etc.Models;
using GiantTeam.Organization.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Organization.Api.Controllers;

[ApiController]
public class FetchInodeByPathController : ControllerBase
{
    [HttpPost("/api/organization/[Controller]")]
    public async Task<Inode> Post(
        [FromServices] FetchInodeService service,
        FetchInodeByPathInput input)
    {
        return await service.FetchInodeByPathAsync(input);
    }
}
