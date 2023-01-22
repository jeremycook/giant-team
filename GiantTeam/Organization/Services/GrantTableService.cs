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

            List<Sql> grantCommands = GenerateGrantTableCommands(schemaName, tableName, table.InodeId, accessControlList);

            // Grant as the pg_database_owner.
            await elevatedDataService.ExecuteAsync(grantCommands.Prepend(Sql.Format($"SET ROLE pg_database_owner")));
        }

        public List<Sql> GenerateGrantTableCommands(string schemaName, string tableName, Guid tableInodeId, IEnumerable<InodeAccess> accessControlList)
        {
            var grantCommands = new List<Sql>();

            foreach (var access in accessControlList)
            {
                grantCommands.Add(Sql.Format($"""
INSERT INTO etc.inode_access (inode_id, role_id, permissions) VALUES ({tableInodeId}, {access.RoleId}, {access.Permissions})
    ON CONFLICT ON CONSTRAINT inode_access_pkey
    DO UPDATE SET permissions = {access.Permissions}
"""));

                var grantTable = new GrantTable(access);

                grantCommands.Add(Sql.Format($"REVOKE ALL ON TABLE {Sql.Identifier(schemaName, tableName)} FROM {Sql.Identifier(grantTable.RoleId)}"));

                if (grantTable.Privileges.Any())
                {
                    grantCommands.Add(Sql.Format($"GRANT {(grantTable.Privileges.Contains(GrantTablePrivilege.ALL) ?
                        Sql.Raw(GrantTablePrivilege.ALL.ToString()) :
                        Sql.Raw(grantTable.Privileges.Select(o => o.ToString()).Join(","))
                    )} ON TABLE {Sql.Identifier(schemaName, tableName)} TO {Sql.Identifier(grantTable.RoleId)}"));
                }
            }

            return grantCommands;
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
                    PermissionId.m => new[] { GrantTablePrivilege.ALL },
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
