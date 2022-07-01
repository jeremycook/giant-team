using System.Security.Claims;
using WebApp.Helpers;

namespace WebApp.Services
{
    public class SessionUser
    {
        public SessionUser(ClaimsIdentity identity)
        {
            Sub =
                identity.Claims.FirstOrDefault(o => o.Type == ClaimsHelper.Types.Sub)?.Value ??
                throw new InvalidOperationException($"Missing \"{ClaimsHelper.Types.Sub}\" claim.");

            Username =
                identity.Claims.FirstOrDefault(o => o.Type == ClaimsHelper.Types.Username)?.Value ??
                throw new InvalidOperationException($"Missing \"{ClaimsHelper.Types.Username}\" claim.");

            DisplayName =
                identity.Claims.FirstOrDefault(o => o.Type == ClaimsHelper.Types.DisplayName)?.Value ??
                throw new InvalidOperationException($"Missing \"{ClaimsHelper.Types.DisplayName}\" claim.");

            DatabaseUsername =
                identity.Claims.FirstOrDefault(o => o.Type == ClaimsHelper.Types.DatabaseUsername)?.Value ??
                throw new InvalidOperationException($"Missing \"{ClaimsHelper.Types.DatabaseUsername}\" claim.");

            DatabasePassword =
                identity.Claims.FirstOrDefault(o => o.Type == ClaimsHelper.Types.DatabasePassword)?.Value ??
                throw new InvalidOperationException($"Missing \"{ClaimsHelper.Types.DatabasePassword}\" claim.");
        }

        public string Sub { get; }
        public string Username { get; }
        public string DisplayName { get; }
        public string DatabaseUsername { get; }
        public string DatabasePassword { get; }
    }
}