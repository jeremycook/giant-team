using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebApp.Data;

namespace WebApp.Services
{
    public class LoginService
    {
        private readonly IDbContextFactory<GiantTeamDbContext> dbContextFactory;
        private readonly EncryptionService encryptionService;

        public LoginService(IDbContextFactory<GiantTeamDbContext> dbContextFactory, EncryptionService encryptionService)
        {
            this.dbContextFactory = dbContextFactory;
            this.encryptionService = encryptionService;
        }

        public async Task<ClaimsPrincipal> LoginAsync(LoginDataModel loginDataModel)
        {
            using var db = await dbContextFactory.CreateDbContextAsync();

            var user = await db.Users
                .SingleOrDefaultAsync(o => o.UsernameLowercase == loginDataModel.Username.ToLower());

            if (user is not null &&
                !string.IsNullOrEmpty(user.PasswordDigest) &&
                encryptionService.VerifyHashedPlaintext(user.PasswordDigest, loginDataModel.Password))
            {
                // TODO: Log authentication success

                var dbPassword = encryptionService.RandomPassword();

                // Set password
                await db.SetUserPasswordAsync(user.UsernameLowercase, dbPassword, DateTimeOffset.UtcNow.AddDays(1));

                var identity = new ClaimsIdentity(authenticationType: "password", nameType: "login", roleType: "role");
                identity.AddClaim(new("sub", user.UserId.ToString()));
                identity.AddClaim(new("login", user.Username));
                identity.AddClaim(new("name", user.DisplayName));
                identity.AddClaim(new("dp", dbPassword));

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
