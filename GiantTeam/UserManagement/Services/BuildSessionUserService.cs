using GiantTeam.Crypto;
using GiantTeam.RecordsManagement.Data;

namespace GiantTeam.UserManagement.Services
{
    public class BuildSessionUserService
    {
        private readonly RecordsManagementDbContext rm;
        private readonly DatabaseSecurityService wa;

        public BuildSessionUserService(
            RecordsManagementDbContext rm,
            DatabaseSecurityService wa)
        {
            this.rm = rm;
            this.wa = wa;
        }

        public async Task<SessionUser> BuildAsync(Guid userId, DateTimeOffset validUntil)
        {
            User user =
                await rm.Users.FindAsync(userId) ??
                throw new ArgumentException($"The provided {nameof(userId)} did not match a user.");

            // Build a session user
            SessionUser sessionUser = new(
                sub: user.UserId.ToString(),
                username: user.Username,
                name: user.Name,
                dbRole: user.DbRoleId,
                dbLogin: $"{user.DbRoleId}:l:{DateTime.UtcNow:yymmddHHmmss}",
                dbPassword: PasswordHelper.GeneratePassword()
            );

            // Create a new database login with a random password and a lifespan
            // that matches the cookie authentication ticket's lifespan
            await wa.CreateLoginAsync(sessionUser, validUntil);

            return sessionUser;
        }
    }
}
