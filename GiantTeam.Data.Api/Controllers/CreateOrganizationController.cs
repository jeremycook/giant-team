using GiantTeam.Organizations.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Data.Api.Controllers;

public class CreateOrganizationController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<CreateOrganizationResult> Post(
        [FromServices] CreateOrganizationService createOrganizationService,
        CreateOrganizationInput input)
    {
        return await createOrganizationService.CreateOrganizationAsync(input);
    }
}