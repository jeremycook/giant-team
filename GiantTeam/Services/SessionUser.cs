using GiantTeam.Data;
using System.Security.Claims;

namespace GiantTeam.Services
{
    public class SessionUser
    {
        public SessionUser(ClaimsIdentity identity, DateTimeOffset? databasePasswordValidUntil = null)
        {
            if (identity is null)
                throw new ArgumentNullException(nameof(identity));

            Sub =
                identity.Claims.FirstOrDefault(o => o.Type == PrincipalHelper.ClaimTypes.Sub)?.Value ??
                throw new InvalidOperationException($"Missing \"{PrincipalHelper.ClaimTypes.Sub}\" claim.");

            Username =
                identity.Claims.FirstOrDefault(o => o.Type == PrincipalHelper.ClaimTypes.Username)?.Value ??
                throw new InvalidOperationException($"Missing \"{PrincipalHelper.ClaimTypes.Username}\" claim.");

            Name =
                identity.Claims.FirstOrDefault(o => o.Type == PrincipalHelper.ClaimTypes.Name)?.Value ??
                throw new InvalidOperationException($"Missing \"{PrincipalHelper.ClaimTypes.Name}\" claim.");

            Email =
                identity.Claims.FirstOrDefault(o => o.Type == PrincipalHelper.ClaimTypes.Email)?.Value ??
                throw new InvalidOperationException($"Missing \"{PrincipalHelper.ClaimTypes.Email}\" claim.");

            EmailVerified = "true" == (
                identity.Claims.FirstOrDefault(o => o.Type == PrincipalHelper.ClaimTypes.EmailVerified)?.Value
            );

            // Database stuff

            DatabaseUsername =
                identity.Claims.FirstOrDefault(o => o.Type == PrincipalHelper.ClaimTypes.DatabaseUsername)?.Value ??
                throw new InvalidOperationException($"Missing \"{PrincipalHelper.ClaimTypes.DatabaseUsername}\" claim.");

            DatabaseSlot = int.Parse(
                identity.Claims.FirstOrDefault(o => o.Type == PrincipalHelper.ClaimTypes.DatabaseSlot)?.Value ??
                throw new InvalidOperationException($"Missing \"{PrincipalHelper.ClaimTypes.DatabaseSlot}\" claim.")
            );

            DatabasePassword =
                identity.Claims.FirstOrDefault(o => o.Type == PrincipalHelper.ClaimTypes.DatabasePassword)?.Value ??
                throw new InvalidOperationException($"Missing \"{PrincipalHelper.ClaimTypes.DatabasePassword}\" claim.");

            DatabasePasswordValidUntil = databasePasswordValidUntil ?? DateTimeOffset.Parse(
                identity.Claims.FirstOrDefault(o => o.Type == PrincipalHelper.ClaimTypes.DatabasePasswordValidUntil)?.Value ??
                throw new InvalidOperationException($"Missing \"{PrincipalHelper.ClaimTypes.DatabasePasswordValidUntil}\" claim.")
            );
        }

        public SessionUser(User user, int databaseSlot, string databasePassword, DateTimeOffset databasePasswordValidUntil)
        {
            Sub = user.UserId.ToString();
            Username = user.Username;
            Name = user.Name;
            Email = user.Email;
            EmailVerified = user.EmailVerified;

            DatabaseUsername = user.UsernameNormalized;
            DatabaseSlot = databaseSlot;
            DatabasePassword = databasePassword;
            DatabasePasswordValidUntil = databasePasswordValidUntil;
        }

        public ClaimsIdentity CreateIdentity()
        {
            ClaimsIdentity identity = new(
                authenticationType: PrincipalHelper.AuthenticationTypes.Password,
                nameType: PrincipalHelper.ClaimTypes.Username,
                roleType: PrincipalHelper.ClaimTypes.Role
            );

            // Common claims
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.Sub, Sub));
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.Name, Name));
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.Username, Username));
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.Email, Email));
            if (EmailVerified) identity.AddClaim(new(PrincipalHelper.ClaimTypes.EmailVerified, "true"));

            // Database login claims
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.DatabaseUsername, DatabaseUsername));
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.DatabaseSlot, DatabaseSlot.ToString()));
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.DatabasePassword, DatabasePassword));
            identity.AddClaim(new(PrincipalHelper.ClaimTypes.DatabasePasswordValidUntil, DatabasePasswordValidUntil.ToString("u")));

            return identity;
        }

        private Guid? _userId;

        public Guid UserId => _userId ??= Guid.Parse(Sub);

        // Common

        public string Sub { get; }
        public string Username { get; }
        public string Name { get; }
        public string Email { get; }
        public bool EmailVerified { get; }

        // Database

        public string DatabaseUsername { get; }
        public int DatabaseSlot { get; }
        public string DatabasePassword { get; }
        public DateTimeOffset DatabasePasswordValidUntil { get; }
    }
}