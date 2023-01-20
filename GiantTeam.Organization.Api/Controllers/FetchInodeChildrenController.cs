using GiantTeam.Organization.Etc.Models;
using GiantTeam.Organization.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Organization.Api.Controllers;

[ApiController]
public class FetchInodeChildrenController : ControllerBase
{
    [HttpPost("/api/organization/[Controller]")]
    public async Task<IEnumerable<Inode>> Post(
        [FromServices] FetchInodeService service,
        FetchInodeChildrenInput input)
    {
        return await service.FetchInodeChildrenAsync(input);
    }
}
