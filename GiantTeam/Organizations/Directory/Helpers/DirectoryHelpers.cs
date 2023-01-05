using Microsoft.AspNetCore.WebUtilities;

namespace GiantTeam.Organizations.Directory.Helpers
{
    public class DirectoryHelpers
    {
        public const string Everyone = "everyone";
        public const string Visitor = "visitor";
        public const string User = "user";

        public static string NormalUserRole(string dbUser)
        {
            return dbUser + ":n";
        }

        public static string ElevatedUserRole(string dbUser)
        {
            return dbUser + ":e";
        }

        public static string OrganizationRole(Guid organizationRoleId)
        {
            return "r:" + WebEncoders.Base64UrlEncode(organizationRoleId.ToByteArray());
        }
    }
}
