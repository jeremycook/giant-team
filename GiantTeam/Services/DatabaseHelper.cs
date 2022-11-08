namespace GiantTeam.Services
{
    public static class DatabaseHelper
    {
        /// <summary>
        /// A team's database role name for DDL operations.
        /// </summary>
        /// <returns></returns>
        public static string DesignRole(string teamName) => $"t:{teamName}:d";

        /// <summary>
        /// A team's database role name for DML operations.
        /// </summary>
        /// <returns></returns>
        public static string ManipulateRole(string teamName) => $"t:{teamName}:m";

        /// <summary>
        /// A team's database role name for DQL operations.
        /// </summary>
        /// <returns></returns>
        public static string QueryRole(string teamName) => $"t:{teamName}:q";

        /// <summary>
        /// The user's database username for DDL operations.
        /// </summary>
        /// <returns></returns>
        public static string DesignUser(string username, int slot = 0) => $"u:{username}:d" + (slot > 0 ? (":s:" + slot) : "");

        /// <summary>
        /// The user's database username for DML operations.
        /// </summary>
        /// <returns></returns>
        public static string ManipulateUser(string username, int slot = 0) => $"u:{username}:m" + (slot > 0 ? (":s:" + slot) : "");

        /// <summary>
        /// The user's database username for DQL operations.
        /// </summary>
        /// <returns></returns>
        public static string QueryUser(string username, int slot = 0) => $"u:{username}:q" + (slot > 0 ? (":s:" + slot) : "");
    }
}
