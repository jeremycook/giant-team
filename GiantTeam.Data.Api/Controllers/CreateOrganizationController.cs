using GiantTeam.Organizations.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Data.Api.Controllers;

public class CreateOrganizationController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<CreateOrganizationResult> Post(
        [FromServices] CreateOrganizationService createOrganizationService,
        CreateOrganizationProps props) =>
        await createOrganizationService.CreateOrganizationAsync(props);
}