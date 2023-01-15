using GiantTeam.Organization.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Organization.Api.Controllers;

[ApiController]
public class CreateSpaceController : ControllerBase
{
    [HttpPost("/api/organization/[Controller]")]
    public async Task<CreateSpaceResult> Post(
        [FromServices] CreateSpaceService service,
        CreateSpaceInput input)
    {
        return await service.CreateSpaceAsync(input);
    }
}