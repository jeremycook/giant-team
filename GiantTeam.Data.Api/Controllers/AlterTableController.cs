using GiantTeam.Workspaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Data.Api.Controllers;

public class AlterTableController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<AlterTable> Post(
        [FromServices] AlterTableService fetchRecordsService,
        AlterTableInput input)
    {
        return await fetchRecordsService.AlterTableAsync(input);
    }
}
