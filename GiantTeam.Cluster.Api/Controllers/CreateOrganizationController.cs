using GiantTeam.Cluster.Directory.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Cluster.Api.Controllers;

public class CreateOrganizationController : ControllerBase
{
    [HttpPost("/api/cluster/[Controller]")]
    public async Task<CreateOrganizationResult> Post(
        [FromServices] CreateOrganizationService createOrganizationService,
        CreateOrganizationInput input)
    {
        return await createOrganizationService.CreateOrganizationAsync(input);
    }
}