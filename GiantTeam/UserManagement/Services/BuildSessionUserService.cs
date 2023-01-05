using GiantTeam.Crypto;
using GiantTeam.Organizations.Directory.Data;
using System.Security.Cryptography;

namespace GiantTeam.UserManagement.Services
{
    public class BuildSessionUserService
    {
        private readonly ManagerDirectoryDbContext directoryManagerDb;
        private readonly DatabaseSecurityService wa;

        public BuildSessionUserService(
            ManagerDirectoryDbContext directoryManagerDb,
            DatabaseSecurityService wa)
        {
            this.directoryManagerDb = directoryManagerDb;
            this.wa = wa;
        }

        /// <summary>
        /// Returns a <see cref="SessionUser"/> with new database login credentials.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="validUntil"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<SessionUser> BuildSessionUserAsync(Guid userId, DateTimeOffset validUntil)
        {
            // Find the user
            User user =
                await directoryManagerDb.Users.FindAsync(userId) ??
                throw new ArgumentException($"The provided {nameof(userId)} did not match a user.");

            // Build a session user
            SessionUser sessionUser = new(
                sub: user.UserId.ToString(),
                username: user.Username,
                dbUser: user.DbUser,
                dbLogin: $"l:{DateTime.UtcNow:yymmddHH}-{Random.Shared.Next()}",
                dbPassword: PasswordHelper.GeneratePassword()
            );

            // Create a new database login with a random password and a lifespan
            // that matches the cookie authentication ticket's lifespan
            await wa.CreateLoginAsync(user, sessionUser.DbLogin, sessionUser.DbPassword, validUntil);

            return sessionUser;
        }
    }
}
