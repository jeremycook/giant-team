using GiantTeam.WorkspaceInteraction.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Data.Api.Controllers;

public class FetchRecordsController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<FetchRecords> Post(
        [FromServices] FetchRecordsService fetchRecordsService,
        FetchRecordsInput input)
    {
        return await fetchRecordsService.FetchRecordsAsync(input);
    }
}
