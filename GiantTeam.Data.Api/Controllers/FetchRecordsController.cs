﻿using GiantTeam.Workspaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Data.Api.Controllers;

public class FetchRecordsController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<FetchRecords> Post(
        [FromServices] FetchRecordsService service,
        FetchRecordsInput input)
    {
        return await service.FetchRecordsAsync(input);
    }
}
