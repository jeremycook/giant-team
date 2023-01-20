using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Linq;
using GiantTeam.Organization.Etc.Models;
using GiantTeam.Postgres;
using GiantTeam.UserData.Services;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Services
{
    public class GrantTableInput
    {
        [RequiredGuid]
        public Guid OrganizationId { get; set; }

        [RequiredGuid]
        public Guid TableInodeId { get; set; }

        [Required]
        public List<InodeAccess> AccessControlList { get; set; } = null!;
    }

    public class GrantTableService
    {
        private readonly ValidationService validationService;
        private readonly FetchInodeService fetchInodeService;
        private readonly UserDataServiceFactory userDataServiceFactory;

        public GrantTableService(
            ValidationService validationService,
            FetchInodeService fetchInodeService,
            UserDataServiceFactory userDataServiceFactory)
        {
            this.validationService = validationService;
            this.fetchInodeService = fetchInodeService;
            this.userDataServiceFactory = userDataServiceFactory;
        }

        public async Task GrantTableAsync(GrantTableInput input)
        {
            validationService.Validate(input);

            try
            {
                var table = await fetchInodeService.FetchInodeAsync(input.OrganizationId, input.TableInodeId);
                var space = await fetchInodeService.FetchInodeByPathAsync(input.OrganizationId, table.Path.Split('/').First());

                await GrantTableAsync(input.OrganizationId, space, table, input.AccessControlList);
            }
            catch (Exception ex)
            {
                throw new ValidationException($"An error occurred that prevented changing access. No changes were made.", ex);
            }
        }

        public async Task GrantTableAsync(Guid organizationId, Inode space, Inode table, IEnumerable<InodeAccess> accessControlList)
        {
            if (table.InodeTypeId != InodeTypeId.Table)
                throw new InvalidOperationException($"Expected a Table but found a {table.InodeTypeId}.");
            if (space.InodeTypeId != InodeTypeId.Space)
                throw new InvalidOperationException($"Expected a Space but found a {space.InodeTypeId}.");

            var elevatedDataService = userDataServiceFactory.NewElevatedDataService(organizationId);

            var schemaName = space.UglyName;
            var tableName = table.UglyName;
            var tableGrants = accessControlList
                .Select(acl => new GrantTable(acl));

            // Grant as the pg_database_owner.
            var commands = new List<Sql>()
            {
                Sql.Format($"SET ROLE pg_database_owner"),
            };
            commands.AddRange(tableGrants.SelectMany(p => new[]
            {
                Sql.Format($"REVOKE ALL ON TABLE {Sql.Identifier(schemaName, tableName)} FROM {Sql.Identifier(p.RoleId)}"),
                Sql.Format($"GRANT {(p.Privileges.Contains(GrantTablePrivilege.ALL) ?
                    Sql.Raw(GrantTablePrivilege.ALL.ToString()) :
                    Sql.Raw(p.Privileges.Select(o => o.ToString()).Join(","))
                )} ON TABLE {Sql.Identifier(schemaName, tableName)} TO {Sql.Identifier(p.RoleId)}"),
            }));

            foreach (var access in accessControlList)
            {
                commands.Add(Sql.Format($"""
INSERT INTO etc.inode_access (inode_id, role_id, permissions) VALUES ({table.InodeId}, {access.RoleId}, {access.Permissions})
    ON CONFLICT ON CONSTRAINT inode_access_pkey
    DO UPDATE SET permissions = {access.Permissions}
"""));
                var grantTable = new GrantTable(access);
                commands.Add(Sql.Format($"REVOKE ALL ON TABLE {Sql.Identifier(schemaName, tableName)} FROM {Sql.Identifier(grantTable.RoleId)}"));
                if (grantTable.Privileges.Any())
                {
                    commands.Add(Sql.Format($"GRANT {(grantTable.Privileges.Contains(GrantTablePrivilege.ALL) ?
                        Sql.Raw(GrantTablePrivilege.ALL.ToString()) :
                        Sql.Raw(grantTable.Privileges.Select(o => o.ToString()).Join(","))
                    )} ON TABLE {Sql.Identifier(schemaName, tableName)} TO {Sql.Identifier(grantTable.RoleId)}"));
                }
            }

            await elevatedDataService.ExecuteAsync(commands);
        }

        internal class GrantTable
        {
            public GrantTable(InodeAccess inodeAccess)
            {
                RoleId = inodeAccess.RoleId;
                Privileges = inodeAccess.Permissions.SelectMany(MapPermissionToGrant).Distinct().ToList();
            }

            public string RoleId { get; set; } = null!;
            public List<GrantTablePrivilege> Privileges { get; set; } = null!;

            public static GrantTablePrivilege[] MapPermissionToGrant(PermissionId permissionId)
            {
                return permissionId switch
                {
                    PermissionId.r => new[] { GrantTablePrivilege.SELECT },
                    PermissionId.a => new[] { GrantTablePrivilege.INSERT },
                    PermissionId.w => new[] { GrantTablePrivilege.UPDATE },
                    PermissionId.d => new[] { GrantTablePrivilege.DELETE },
                    PermissionId.D => new[] { GrantTablePrivilege.TRUNCATE },
                    PermissionId.x => Array.Empty<GrantTablePrivilege>(),
                    PermissionId.N => new[] { GrantTablePrivilege.ALL },
                    PermissionId.C => Array.Empty<GrantTablePrivilege>(),
                    PermissionId.o => Array.Empty<GrantTablePrivilege>(),
                    _ => throw new NotImplementedException(permissionId.ToString()),
                };
            }
        }

        internal enum GrantTablePrivilege
        {
            SELECT = 1,
            INSERT = 2,
            UPDATE = 3,
            DELETE = 4,
            TRUNCATE = 5,
            REFERENCES = 6,
            TRIGGER = 7,
            ALL = 100,
        }
    }
}
