using GiantTeam.Workspaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Data.Api.Controllers;

public class CreateTableController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<CreateTableOutput> Post(
        [FromServices] CreateTableService fetchRecordsService,
        CreateTableInput input)
    {
        return await fetchRecordsService.CreateTableAsync(input);
    }
}
