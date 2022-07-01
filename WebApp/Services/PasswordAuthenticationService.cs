using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebApp.Data;
using WebApp.Helpers;

namespace WebApp.Services
{
    public class PasswordAuthenticationService
    {
        private readonly IDbContextFactory<GiantTeamDbContext> dbContextFactory;
        private readonly HashingService hashingService;

        public PasswordAuthenticationService(IDbContextFactory<GiantTeamDbContext> dbContextFactory, HashingService hashingService)
        {
            this.dbContextFactory = dbContextFactory;
            this.hashingService = hashingService;
        }

        public async Task<ClaimsPrincipal> AuthenticateAsync(PasswordAuthenticationInput input)
        {
            using var db = await dbContextFactory.CreateDbContextAsync();

            var user = await db.Users
                .SingleOrDefaultAsync(o => o.UsernameLowercase == input.Username.ToLower());

            if (user is not null &&
                !string.IsNullOrEmpty(user.PasswordDigest) &&
                hashingService.VerifyHashedPlaintext(user.PasswordDigest, input.Password))
            {
                // TODO: Log authentication success

                // Set password
                var dbPassword = hashingService.RandomPassword();
                await db.SetUserPasswordAsync(user.DatabaseUsername, dbPassword, DateTimeOffset.UtcNow.AddDays(1));

                // Create identity
                ClaimsIdentity identity = new(authenticationType: "password", nameType: ClaimsHelper.Types.Username, roleType: ClaimsHelper.Types.Role);
                identity.AddClaim(new(ClaimsHelper.Types.Sub, user.UserId.ToString()));
                identity.AddClaim(new(ClaimsHelper.Types.Username, user.Username));
                identity.AddClaim(new(ClaimsHelper.Types.DisplayName, user.DisplayName));
                identity.AddClaim(new(ClaimsHelper.Types.DatabaseUsername, user.DatabaseUsername));
                identity.AddClaim(new(ClaimsHelper.Types.DatabasePassword, dbPassword));

                var principal = new ClaimsPrincipal(identity);

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
