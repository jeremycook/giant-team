using GiantTeam.Cluster.Directory.Services;
using GiantTeam.Postgres.Models;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Cluster.Api.Controllers;

[ApiController]
public class QueryDirectoryController : ControllerBase
{
    [HttpPost("/api/cluster/[Controller]")]
    public async Task<QueryTable> Post(
        [FromServices] QueryDirectoryService service,
        QueryDirectoryInput input)
    {
        return await service.QueryDirectoryAsync(input);
    }
}
