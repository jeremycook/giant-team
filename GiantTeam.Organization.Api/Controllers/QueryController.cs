using GiantTeam.Organization.Services;
using GiantTeam.Postgres.Models;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Organization.Api.Controllers;

[ApiController]
public class QueryController : ControllerBase
{
    [HttpPost("/api/organization/[Controller]")]
    public async Task<TabularData> Post(
        [FromServices] QueryService service,
        QueryInput input)
    {
        return await service.QueryAsync(input);
    }
}
