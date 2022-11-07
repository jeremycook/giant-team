using System.Text.RegularExpressions;

namespace GiantTeam.DatabaseModel
{
    //GRANT ALL ON SCHEMA "Finance" TO "t:DB1:d";
    //GRANT USAGE ON SCHEMA "Finance" TO "t:DB1:m";
    //GRANT USAGE ON SCHEMA "Finance" TO "t:DB1:q";

    /// <summary>
    /// 
    /// </summary>
    public class SchemaPrivileges
    {
        public SchemaPrivileges(string privileges, string grantee)
        {
            Grantee = grantee;
            this.privileges = privileges;
        }

        public string Grantee { get; }

        private readonly string privileges;
        private bool privilegesIsValid;
        /// <summary>
        /// Examples: ALL, CREATE, USAGE, CREATE USAGE
        /// </summary>
        public string Privileges
        {
            get
            {
                if (privilegesIsValid)
                {
                    return privileges;
                }
                else if (Regex.IsMatch(privileges, "^[A-Z ]+$"))
                {
                    privilegesIsValid = true;
                    return privileges;
                }
                else
                {
                    throw new InvalidOperationException($"The value \"{privileges}\" of {nameof(Privileges)} is invalid.");
                }
            }
        }
    }
}