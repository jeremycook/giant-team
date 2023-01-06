using GiantTeam.Organizations.Organization.Services;
using GiantTeam.Postgres.Models;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Data.Api.Controllers;

public class QueryDatabaseController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<QueryTable> Post(
        [FromServices] QueryDatabaseService service,
        QueryDatabaseInput input)
    {
        return await service.QueryDatabaseAsync(input);
    }
}
