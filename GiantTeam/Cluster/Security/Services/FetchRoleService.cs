using GiantTeam.Cluster.Directory.Services;
using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Postgres;
using GiantTeam.UserManagement.Services;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Cluster.Security.Services
{
    public class FetchRoleService
    {
        private readonly ValidationService validationService;
        private readonly SessionService sessionService;
        private readonly DirectoryManagementService dataService;

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
            DirectoryManagementService dataService)
        {
            this.validationService = validationService;
            this.sessionService = sessionService;
            this.dataService = dataService;
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

            var output = await dataService.SingleOrDefaultAsync<FetchRoleOutput>(Sql.Format($"""
select
    rolname {Sql.Identifier(nameof(FetchRoleOutput.RoleName))},
    rolcanlogin {Sql.Identifier(nameof(FetchRoleOutput.CanLogin))},
    rolcreatedb {Sql.Identifier(nameof(FetchRoleOutput.CreateDb))},
    rolinherit {Sql.Identifier(nameof(FetchRoleOutput.Inherit))}
from pg_catalog.pg_roles
where rolname = {input.RoleName};
"""));

            if (output is null)
            {
                throw new NotFoundException($"Role not found.");
            }

            output.Members = await dataService.ListAsync<FetchRoleMemberOutput>(Sql.Format($"""
select
    r.rolname as {Sql.Identifier(nameof(FetchRoleMemberOutput.RoleName))},
	admin_option as {Sql.Identifier(nameof(FetchRoleMemberOutput.TeamAdmin))},
	r.rolinherit as {Sql.Identifier(nameof(FetchRoleMemberOutput.Inherit))}
from pg_catalog.pg_roles team
join pg_catalog.pg_auth_members m on m.member = team.oid
join pg_catalog.pg_roles r on m.roleid = r.oid
where team.rolname = {input.RoleName}
order by 1;
"""));

            return output;
        }
    }
}
