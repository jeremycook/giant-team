using GiantTeam.Organization.Etc.Models;
using GiantTeam.Organization.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Organization.Api.Controllers;

[ApiController]
public class FetchInodeListController : ControllerBase
{
    [HttpPost("/api/organization/[Controller]")]
    public async Task<IEnumerable<Inode>> Post(
        [FromServices] FetchInodeService service,
        FetchInodeListInput input)
    {
        return await service.FetchInodeListAsync(input);
    }
}
