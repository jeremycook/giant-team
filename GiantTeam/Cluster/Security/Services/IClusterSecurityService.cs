using GiantTeam.UserManagement.Services;

namespace GiantTeam.Cluster.Security.Services
{
    public interface IClusterSecurityService
    {
        Task CreateUserRolesAsync(string dbUser);
        Task CreateLoginAsync(string dbUser, string dbLogin, string dbLoginPassword, DateTime passwordValidUntil);
        Task SetLoginExpirationAsync(SessionUser user, DateTime passwordValidUntil);
    }
}
