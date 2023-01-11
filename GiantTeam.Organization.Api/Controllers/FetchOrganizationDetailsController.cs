using GiantTeam.Organization.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Organization.Api.Controllers;

public class FetchOrganizationDetailsController : ControllerBase
{
    [HttpPost("/api/organization/[Controller]")]
    public async Task<FetchOrganizationDetailsResult> Post(
        [FromServices] FetchOrganizationDetailsService service,
        FetchOrganizationDetailsInput input)
    {
        return await service.FetchOrganizationDetailsAsync(input);
    }
}