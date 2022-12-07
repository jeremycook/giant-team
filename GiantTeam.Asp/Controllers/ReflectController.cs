using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Authentication.Api.Controllers;

[AllowAnonymous]
public class ReflectController : ControllerBase
{
    public class ReflectOutput
    {
        public string Scheme { get; set; } = null!;
        public string Host { get; set; } = null!;
        public string Protocol { get; set; } = null!;
        public string? RemoteIpAddress { get; set; } = null!;
    }

    [HttpGet("/api/[Controller]")]
    public ReflectOutput Get()
    {
        return new()
        {
            Scheme = Request.Scheme,
            Host = Request.Host.Value,
            Protocol = Request.Protocol,
            RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
        };
    }
}
