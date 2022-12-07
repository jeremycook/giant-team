using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Authentication.Api.Controllers;

[AllowAnonymous]
public class ReflectController : ControllerBase
{
    public class ReflectOutput
    {
        public string BaseUrl { get; set; } = null!;
        public string Protocol { get; set; } = null!;
        public string? Ip { get; set; } = null!;
    }

    [HttpGet("/api/[Controller]")]
    public ReflectOutput Get()
    {
        return new()
        {
            BaseUrl = Request.Scheme + "://" + Request.Host.Value + Request.PathBase,
            Protocol = Request.Protocol,
            Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
        };
    }
}
