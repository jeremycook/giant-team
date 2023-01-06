using GiantTeam.Organization.Services;
using GiantTeam.Postgres.Models;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Organization.Api.Controllers;

public class QueryDatabaseController : ControllerBase
{
    [HttpPost("/api/organization/[Controller]")]
    public async Task<QueryTable> Post(
        [FromServices] QueryDatabaseService service,
        QueryDatabaseInput input)
    {
        return await service.QueryDatabaseAsync(input);
    }
}
