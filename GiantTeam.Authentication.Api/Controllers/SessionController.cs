using GiantTeam.UserManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        public Guid? UserId { get; set; }
        public string? Username { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public bool? EmailVerified { get; set; }
    }

    public enum SessionStatus
    {
        Anonymous = 0,
        /// <summary>
        /// The user is authenticated.
        /// Check out the other <see cref="SessionOutput"/> properties.
        /// </summary>
        Authenticated = 1,
    }

    [HttpPost("/api/[Controller]")]
    public async Task<SessionOutput> Post(
        [FromServices] SessionService sessionService)
    {
        if (User.Identity is null || !User.Identity.IsAuthenticated)
        {
            return new(SessionStatus.Anonymous);
        }
        else
        {
            var user = sessionService.User;
            return new(SessionStatus.Authenticated)
            {
                UserId = user.UserId,
                Name = user.Name,
                Username = user.Username,
                Email = user.Email,
                EmailVerified = user.EmailVerified,
            };
        }
    }
}
