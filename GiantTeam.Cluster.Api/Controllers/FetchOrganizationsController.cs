using GiantTeam.Cluster.Directory.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Cluster.Api.Controllers;

public class FetchOrganizationsController : ControllerBase
{
    [HttpPost("/api/cluster/[Controller]")]
    public async Task<FetchOrganizationsOutput> Post(
        [FromServices] FetchOrganizationsService service)
    {
        return await service.FetchOrganizationsAsync();
    }
}
