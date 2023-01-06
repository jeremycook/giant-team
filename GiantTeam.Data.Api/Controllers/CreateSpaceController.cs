using GiantTeam.Organizations.Organization.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Data.Api.Controllers;

public class CreateSpaceController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<CreateSpaceResult> Post(
        [FromServices] CreateSpaceService service,
        CreateSpaceInput input)
    {
        return await service.CreateSpaceAsync(input);
    }
}