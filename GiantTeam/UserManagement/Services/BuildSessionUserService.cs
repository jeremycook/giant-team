using GiantTeam.Cluster.Directory.Data;
using GiantTeam.Cluster.Security.Services;
using GiantTeam.Crypto;
using Microsoft.EntityFrameworkCore;

namespace GiantTeam.UserManagement.Services
{
    public class BuildSessionUserService
    {
        private readonly IDbContextFactory<ManagerDirectoryDbContext> managerDirectoryDbContextFactory;
        private readonly ClusterSecurityService securityService;

        public BuildSessionUserService(
            IDbContextFactory<ManagerDirectoryDbContext> managerDirectoryDbContextFactory,
            ClusterSecurityService securityService)
        {
            this.managerDirectoryDbContextFactory = managerDirectoryDbContextFactory;
            this.securityService = securityService;
        }

        /// <summary>
        /// Returns a <see cref="SessionUser"/> with new database login credentials.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="validUntil"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<SessionUser> BuildSessionUserAsync(bool elevated, Guid userId, DateTime validUntil)
        {
            await using var directoryManagerDb = await managerDirectoryDbContextFactory.CreateDbContextAsync();

            // Find the user
            User user =
                await directoryManagerDb.Users.FindAsync(userId) ??
                throw new ArgumentException($"The provided {nameof(userId)} did not match a user.");

            // Build a session user
            SessionUser sessionUser = new(
                sub: user.UserId.ToString(),
                username: user.Username,
                elevated: elevated,
                dbUser: user.DbUser,
                dbLogin: $"l:{DateTime.UtcNow:yymmddHH}-{Random.Shared.Next()}",
                dbPassword: PasswordHelper.GeneratePassword()
            );

            // Create regular database login
            await securityService.CreateLoginAsync(
                dbUser: user.DbUser,
                dbLogin: sessionUser.DbLogin,
                dbLoginPassword: sessionUser.DbPassword(),
                passwordValidUntil: validUntil
            );

            if (sessionUser.DbElevatedUser is not null &&
                sessionUser.DbElevatedLogin is not null)
            {
                // Create elevated database login
                await securityService.CreateLoginAsync(
                    dbUser: sessionUser.DbElevatedUser,
                    dbLogin: sessionUser.DbElevatedLogin,
                    dbLoginPassword: sessionUser.DbPassword(),
                    passwordValidUntil: validUntil
                );
            }

            return sessionUser;
        }
    }
}
