using GiantTeam.Crypto;
using GiantTeam.Organizations.Directory.Data;

namespace GiantTeam.UserManagement.Services
{
    public class BuildSessionUserService
    {
        private readonly ManagerDirectoryDbContext directoryManagerDb;
        private readonly ClusterSecurityService wa;

        public BuildSessionUserService(
            ManagerDirectoryDbContext directoryManagerDb,
            ClusterSecurityService wa)
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
        public async Task<SessionUser> BuildSessionUserAsync(bool elevated, Guid userId, DateTimeOffset validUntil)
        {
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

            // Create database logins
            await wa.CreateLoginAsync(
                dbUser: user.DbUser,
                dbLogin: sessionUser.DbLogin,
                loginPassword: sessionUser.DbPassword,
                validUntil: validUntil
            );

            if (sessionUser.DbElevatedUser is not null &&
                sessionUser.DbElevatedLogin is not null)
            {
                // Create elevated database logins
                await wa.CreateLoginAsync(
                    dbUser: sessionUser.DbElevatedUser,
                    dbLogin: sessionUser.DbElevatedLogin,
                    loginPassword: sessionUser.DbPassword,
                    validUntil: validUntil
                );
            }

            return sessionUser;
        }
    }
}
