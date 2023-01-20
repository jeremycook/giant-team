using Microsoft.AspNetCore.WebUtilities;

namespace GiantTeam.Cluster.Directory.Helpers
{
    public static class DirectoryHelpers
    {
        public const string Database = "directory";
        public const string Schema = "directory";

        public const string Anyone = "anyone";
        public const string Anyvisitor = "anyvisitor";
        public const string Anyuser = "anyuser";

        public static string ElevatedUserRole(string dbUser)
        {
            return dbUser + ":e";
        }

        public static string ElevatedLogin(string dbLogin)
        {
            return dbLogin + ":e";
        }

        public static string OrganizationRole(Guid roleId)
        {
            if (roleId == Guid.Empty)
            {
                throw new ArgumentException($"The {nameof(roleId)} argument cannot be all-zeroes.", nameof(roleId));
            }

            return $"r:{roleId:n}";
        }
    }
}
