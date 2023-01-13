using GiantTeam.Organization.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Organization.Api.Controllers;

public class FetchInodeController : ControllerBase
{
    [HttpPost("/api/organization/[Controller]")]
    public async Task<FetchInodeResult> Post(
        [FromServices] FetchInodeService service,
        FetchInodeInput input)
    {
        return await service.FetchInodeAsync(input);
    }
}
