using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebApp.Data;

namespace WebApp.Services
{
    public class PasswordAuthenticationService
    {
        private readonly IDbContextFactory<GiantTeamDbContext> dbContextFactory;
        private readonly IOptions<CookieAuthenticationOptions> cookieAuthenticationOptions;

        public PasswordAuthenticationService(
            IDbContextFactory<GiantTeamDbContext> dbContextFactory,
            IOptions<CookieAuthenticationOptions> cookieAuthenticationOptions)
        {
            this.dbContextFactory = dbContextFactory;
            this.cookieAuthenticationOptions = cookieAuthenticationOptions;
        }

        public async Task<ClaimsPrincipal> AuthenticateAsync(PasswordAuthenticationInput input)
        {
            using var db = await dbContextFactory.CreateDbContextAsync();

            var user = await db.Users
                .SingleOrDefaultAsync(o => o.Username == input.Username.ToLower());

            if (user is not null &&
                !string.IsNullOrEmpty(user.PasswordDigest) &&
                HashingHelper.VerifyHashedPlaintext(user.PasswordDigest, input.Password))
            {
                // TODO: Log authentication success

                // TODO: Capture or create a free slot instead of always using slot 1.
                int databaseSlot = 1;
                string databasePassword = PasswordHelper.Base64Url();
                // Match the lifespan of the database password to that of the cookie authentication ticket
                DateTimeOffset databasePasswordValidUntil = DateTimeOffset.UtcNow.Add(cookieAuthenticationOptions.Value.ExpireTimeSpan);

                // Create session user
                SessionUser sessionUser = new(user, databaseSlot, databasePassword, databasePasswordValidUntil);

                // Set database passwords
                await db.SetDatabaseUserPasswordsAsync(sessionUser.DatabaseUsername, sessionUser.DatabaseSlot, sessionUser.DatabasePassword, sessionUser.DatabasePasswordValidUntil);

                // Create principal
                ClaimsPrincipal principal = new(sessionUser.CreateIdentity());

                return principal;
            }
            else
            {
                // TODO: Log authentication failure
                throw new ValidationException($"The username or password is incorrect.");
            }
        }

    }
}
