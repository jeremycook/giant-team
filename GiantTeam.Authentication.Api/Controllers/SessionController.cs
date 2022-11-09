using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace GiantTeam.Authentication.Api.Controllers;

[AllowAnonymous]
public class SessionController : ControllerBase
{
    public class SessionOutput
    {
        public SessionOutput(SessionStatus status)
        {
            Status = status;
        }

        public SessionStatus Status { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; init; }
    }

    public enum SessionStatus
    {
        Anonymous = 0,
        Authenticated = 1,
    }

    [HttpPost("/api/[Controller]")]
    public async Task<SessionOutput> Post()
    {
        if (User.Identity is null || !User.Identity.IsAuthenticated)
        {
            return new(SessionStatus.Anonymous);
        }
        else
        {
            return new(SessionStatus.Authenticated)
            {
                Name = User.Identity.Name,
            };
        }
    }
}
