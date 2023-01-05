using GiantTeam.Databases.Database.Services;
using GiantTeam.Postgres.Models;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Data.Api.Controllers;

public class QueryDatabaseController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<QueryTable> Post(
        [FromServices] QueryDatabaseService service,
        QueryDatabaseProps props)
    {
        return await service.QueryDatabaseAsync(props);
    }
}
