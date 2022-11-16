using GiantTeam.Crypto;
using GiantTeam.RecordsManagement.Data;
using GiantTeam.UserManagement.Services;
using GiantTeam.WorkspaceAdministration.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace GiantTeam.Asp.Services
{
    public class PasswordAuthenticationService
    {
        private readonly RecordsManagementDbContext db;
        private readonly WorkspaceAdministrationDbContext databaseAdministrationDbContext;
        private readonly IOptions<CookieAuthenticationOptions> cookieAuthenticationOptions;

        public PasswordAuthenticationService(
            RecordsManagementDbContext db,
            WorkspaceAdministrationDbContext databaseAdministrationDbContext,
            IOptions<CookieAuthenticationOptions> cookieAuthenticationOptions)
        {
            this.db = db;
            this.databaseAdministrationDbContext = databaseAdministrationDbContext;
            this.cookieAuthenticationOptions = cookieAuthenticationOptions;
        }

        public async Task<ClaimsPrincipal> AuthenticateAsync(PasswordAuthenticationInput input)
        {
            var user = await db.Users
                .SingleOrDefaultAsync(o => o.InvariantUsername == input.Username.ToLowerInvariant());

            if (user is not null &&
                !string.IsNullOrEmpty(user.PasswordDigest) &&
                HashingHelper.VerifyHashedPlaintext(user.PasswordDigest, input.Password))
            {
                // TODO: Log authentication success

                string dbLogin = await databaseAdministrationDbContext.CreateDatabaseLoginAsync(user.DbRoleId);

                string dbPassword = PasswordHelper.Base64Url();

                // Match the lifespan of the database password to that of the cookie authentication ticket
                DateTimeOffset validUntil = DateTimeOffset.UtcNow.Add(cookieAuthenticationOptions.Value.ExpireTimeSpan);

                await databaseAdministrationDbContext.SetDatabasePasswordsAsync(dbLogin, dbPassword, validUntil);

                var sessionUser = new SessionUser(
                    sub: user.UserId.ToString(),
                    username: user.Username,
                    name: user.Name,
                    email: user.Email,
                    emailVerified: user.EmailVerified,
                    dbLogin: dbLogin,
                    dbPassword: dbPassword,
                    dbRole: user.DbRoleId
                );

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
