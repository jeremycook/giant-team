using GiantTeam.Organization.Etc.Models;
using GiantTeam.Organization.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Organization.Api.Controllers;

[ApiController]
public class CreateTableController : ControllerBase
{
    [HttpPost("/api/organization/[Controller]")]
    public async Task<Inode> Post(
        [FromServices] CreateTableService service,
        CreateTableInput input)
    {
        return await service.CreateTableAsync(input);
    }
}