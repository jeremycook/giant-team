using GiantTeam.Workspaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Data.Api.Controllers;

public class AlterTableController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<AlterTable> Post(
        [FromServices] AlterTableService service,
        AlterTableInput input)
    {
        return await service.AlterTableAsync(input);
    }
}
