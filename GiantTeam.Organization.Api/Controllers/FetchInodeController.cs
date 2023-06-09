﻿using GiantTeam.Organization.Etc.Models;
using GiantTeam.Organization.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Organization.Api.Controllers;

[ApiController]
public class FetchInodeController : ControllerBase
{
    [HttpPost("/api/organization/[Controller]")]
    public async Task<Inode> Post(
        [FromServices] FetchInodeService service,
        FetchInodeInput input)
    {
        return await service.FetchInodeAsync(input);
    }
}
