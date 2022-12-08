using GiantTeam.WorkspaceAdministration.Services;
using Microsoft.AspNetCore.Mvc;
using static GiantTeam.WorkspaceAdministration.Services.FetchRecordsService;

namespace GiantTeam.Data.Api.Controllers;

public class FetchRecordsController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<FetchRecordsOutput> Post(
        [FromServices] FetchRecordsService fetchRecordsService,
        [FromBody] FetchRecordsInput input)
    {
        return await fetchRecordsService.FetchRecordsAsync(input);
    }
}
