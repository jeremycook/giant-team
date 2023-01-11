using GiantTeam.Organization.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Organization.Api.Controllers;

public class ExploreController : ControllerBase
{
    [HttpPost("/api/organization/[Controller]")]
    public async Task<ExploreResult> Post(
        [FromServices] ExploreService service,
        ExploreInput input)
    {
        return await service.ExploreAsync(input);
    }
}
