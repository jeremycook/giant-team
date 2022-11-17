using GiantTeam.RecordsManagement.Data;
using GiantTeam.WorkspaceAdministration.Data;

namespace GiantTeam.UserManagement.Services
{
    public class BuildSessionUserService
    {
        private readonly RecordsManagementDbContext rm;
        private readonly WorkspaceAdministrationDbContext wa;

        public BuildSessionUserService(
            RecordsManagementDbContext rm,
            WorkspaceAdministrationDbContext wa)
        {
            this.rm = rm;
            this.wa = wa;
        }

        public async Task<SessionUser> BuildAsync(Guid userId, DateTimeOffset validUntil)
        {
            User user =
                await rm.Users.FindAsync(userId) ??
                throw new ArgumentException($"The provided {nameof(userId)} did not match a user.");

            // Create a new database login with a random password and a lifespan
            // that matches the cookie authentication ticket's lifespan
            string dbLogin = await wa.CreateDatabaseLoginAsync(user.DbRoleId);
            string dbPassword = PasswordHelper.Base64Url();
            await wa.SetDatabasePasswordsAsync(dbLogin, dbPassword, validUntil);

            // Build a session user
            SessionUser sessionUser = new(
                sub: user.UserId.ToString(),
                username: user.Username,
                name: user.Name,
                dbLogin: dbLogin,
                dbPassword: dbPassword,
                dbRole: user.DbRoleId
            );

            return sessionUser;
        }
    }
}
