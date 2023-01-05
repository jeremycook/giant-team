﻿using GiantTeam.Organizations.Directory.Models;
using GiantTeam.Organizations.Directory.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Data.Api.Controllers;

public class FetchOrganizationController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<Organization> Post(
        [FromServices] FetchOrganizationService service,
        FetchOrganizationInput input)
    {
        return await service.FetchOrganizationAsync(input);
    }
}
