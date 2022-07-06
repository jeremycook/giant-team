namespace WebApp.Services
{
    public static class UserHelper
    {
        /// <summary>
        /// The user's database username for DDL operations.
        /// </summary>
        /// <returns></returns>
        public static string DDL(string username, int slot = 0) => $"u:{username}:d" + (slot > 0 ? (":" + slot) : "");

        /// <summary>
        /// The user's database username for DML operations.
        /// </summary>
        /// <returns></returns>
        public static string DML(string username, int slot = 0) => $"u:{username}:m" + (slot > 0 ? (":" + slot) : "");

        /// <summary>
        /// The user's database username for DQL operations.
        /// </summary>
        /// <returns></returns>
        public static string DQL(string username, int slot = 0) => $"u:{username}:q" + (slot > 0 ? (":" + slot) : "");
    }
}
