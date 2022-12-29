using Dapper;
using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Models;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Postgres;
using GiantTeam.UserManagement.Services;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.WorkspaceAdministration.Services
{
    public class FetchRoleService
    {
        private readonly ValidationService validationService;
        private readonly SessionService sessionService;
        private readonly UserConnectionService connectionService;

        public class FetchRoleInput
        {
            [Required]
            [StringLength(50), PgIdentifier]
            public string? RoleName { get; set; }
        }

        public class FetchRoleOutput
        {
            public string RoleName { get; set; } = null!;
            public bool CanLogin { get; set; }
            public bool CreateDb { get; set; }
            public bool Inherit { get; set; }
            public IEnumerable<FetchRoleMemberOutput> Members { get; set; } = null!;
        }

        public class FetchRoleMemberOutput
        {
            public string RoleName { get; set; } = null!;
            public bool Inherit { get; set; }
            public bool TeamAdmin { get; set; }
        }

        public FetchRoleService(
            ValidationService validationService,
            SessionService sessionService,
            UserConnectionService connectionService)
        {
            this.validationService = validationService;
            this.sessionService = sessionService;
            this.connectionService = connectionService;
        }

        /// <summary>
        /// Fetch the requested role. Throws <see cref="DetailedValidationException"/> if not found.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException">Role not found.</exception>
        public async Task<FetchRoleOutput> FetchRoleAsync(FetchRoleInput input)
        {
            validationService.Validate(input);

            var user = sessionService.User;

            using var connection = await connectionService.OpenInfoConnectionAsync(user.DbRole);

            var gridReader = await connection.QueryMultipleAsync($"""
select rolname {PgQuote.Identifier(nameof(FetchRoleOutput.RoleName))},
    rolcanlogin {PgQuote.Identifier(nameof(FetchRoleOutput.CanLogin))},
    rolcreatedb {PgQuote.Identifier(nameof(FetchRoleOutput.CreateDb))},
    rolinherit {PgQuote.Identifier(nameof(FetchRoleOutput.Inherit))}
from pg_catalog.pg_roles
where rolname = @RoleName;

select r.rolname as {PgQuote.Identifier(nameof(FetchRoleMemberOutput.RoleName))},
	admin_option as {PgQuote.Identifier(nameof(FetchRoleMemberOutput.TeamAdmin))},
	r.rolinherit as {PgQuote.Identifier(nameof(FetchRoleMemberOutput.Inherit))}
from pg_catalog.pg_roles team
join pg_catalog.pg_auth_members m on m.member = team.oid
join pg_catalog.pg_roles r on m.roleid = r.oid
where team.rolname = @RoleName
order by 1;
""",
new
{
    RoleName = input.RoleName,
});

            var output = gridReader.ReadSingleOrDefault<FetchRoleOutput>();

            if (output is null)
            {
                throw new NotFoundException($"Role not found.");
            }

            output.Members = await gridReader.ReadAsync<FetchRoleMemberOutput>();

            return output;
        }
    }
}
