using GiantTeam.Organization.Etc.Models;
using GiantTeam.Organization.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Organization.Api.Controllers;

[ApiController]
public class FetchOrganizationDetailsController : ControllerBase
{
    [HttpPost("/api/organization/[Controller]")]
    public async Task<OrganizationDetails> Post(
        [FromServices] FetchOrganizationDetailsService service,
        FetchOrganizationDetailsInput input)
    {
        return await service.FetchOrganizationDetailsAsync(input);
    }
}