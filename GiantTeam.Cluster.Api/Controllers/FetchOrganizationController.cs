using GiantTeam.Organizations.Directory.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Cluster.Api.Controllers;

public class FetchOrganizationController : ControllerBase
{
    [HttpPost("/api/cluster/[Controller]")]
    public async Task<FetchOrganizationOutput> Post(
        [FromServices] FetchOrganizationService service,
        FetchOrganizationInput input)
    {
        return await service.FetchOrganizationAsync(input);
    }
}
