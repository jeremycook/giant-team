using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiantTeam.Organization.Etc.Models
{
    public static class SchemaPermissionId
    {
        public const string ALL = "ALL";
        public const string CREATE = "CREATE";
        public const string USAGE = "USAGE";

        public const char R_USAGE = 'r';
        public const char A_CREATE = 'a';

        public static Dictionary<char, string[]> Map { get; } = new()
        {
            { R_USAGE, new[]{"USAGE"} },
            { A_CREATE, new[]{"CREATE"}  },
        };
    }
}
