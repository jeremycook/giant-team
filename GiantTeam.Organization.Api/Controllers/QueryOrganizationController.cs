using GiantTeam.Organization.Services;
using GiantTeam.Postgres.Models;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Organization.Api.Controllers;

[ApiController]
public class QueryOrganizationController : ControllerBase
{
    [HttpPost("/api/organization/[Controller]")]
    public async Task<QueryTable> Post(
        [FromServices] QueryOrganizationService service,
        QueryOrganizationInput input)
    {
        return await service.QueryOrganizationAsync(input);
    }
}
