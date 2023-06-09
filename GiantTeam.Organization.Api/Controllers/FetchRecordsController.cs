﻿using GiantTeam.Organization.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Organization.Api.Controllers;

[ApiController]
public class FetchRecordsController : ControllerBase
{
    [HttpPost("/api/organization/[Controller]")]
    public async Task<FetchRecords> Post(
        [FromServices] FetchRecordsService service,
        FetchRecordsInput input)
    {
        return await service.FetchRecordsAsync(input);
    }
}
