using GiantTeam.Workspaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Data.Api.Controllers;

public class CreateTableController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<CreateTableOutput> Post(
        [FromServices] CreateTableService service,
        CreateTableInput input)
    {
        return await service.CreateTableAsync(input);
    }
}
