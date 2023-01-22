using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organization.Etc.Models;
using GiantTeam.Postgres;
using GiantTeam.UserData.Services;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Services
{
    public class GrantSpaceInput
    {
        [RequiredGuid]
        public Guid OrganizationId { get; set; }

        [RequiredGuid]
        public Guid InodeId { get; set; }

        [Required]
        public InodeAccess[] AccessControlList { get; set; } = null!;
    }

    public class GrantSpaceService
    {
        private readonly ValidationService validationService;
        private readonly FetchInodeService fetchInodeService;
        private readonly UserDataServiceFactory userDataServiceFactory;

        public GrantSpaceService(
            ValidationService validationService,
            FetchInodeService fetchInodeService,
            UserDataServiceFactory userDataServiceFactory)
        {
            this.validationService = validationService;
            this.fetchInodeService = fetchInodeService;
            this.userDataServiceFactory = userDataServiceFactory;
        }

        public async Task GrantSpaceAsync(GrantSpaceInput input)
        {
            validationService.Validate(input);

            var space = await fetchInodeService.FetchInodeAsync(input.OrganizationId, input.InodeId);
            if (space.InodeTypeId != InodeTypeId.Space)
            {
                throw new InvalidOperationException("The inode is not a space.");
            }

            await GrantSpaceAsync(input.OrganizationId, space.InodeId, space.UglyName, input.AccessControlList);
        }

        public async Task GrantSpaceAsync(
            Guid organizationId,
            Guid schemaInodeId,
            string schemaName,
            InodeAccess[] accessControlList)
        {
            var elevatedDataService = userDataServiceFactory.NewElevatedDataService(organizationId);

            // Grant the SCHEMA as the pg_database_owner.
            var commands = new List<Sql>()
            {
                Sql.Format($"SET ROLE pg_database_owner"),
            };

            foreach (var access in accessControlList)
            {
                commands.Add(Sql.Format($"""
INSERT INTO etc.inode_access (inode_id, role_id, permissions) VALUES ({schemaInodeId}, {access.RoleId}, {access.Permissions})
    ON CONFLICT ON CONSTRAINT inode_access_pkey
    DO UPDATE SET permissions = {access.Permissions}
"""));
                commands.Add(Sql.Format($"REVOKE ALL ON SCHEMA {Sql.Identifier(schemaName)} FROM {Sql.Identifier(access.RoleId)}"));

                var schemaPrivileges = access.Permissions
                    .SelectMany(MapPermissionToSchemaPrivilege)
                    .Distinct()
                    .ToArray();

                if (schemaPrivileges.Any())
                {
                    commands.Add(Sql.Format($"GRANT {Sql.Raw(string.Join(',', schemaPrivileges))} ON SCHEMA {Sql.Identifier(schemaName)} TO {Sql.Identifier(access.RoleId)}"));
                }
            }

            await elevatedDataService.ExecuteAsync(commands);
        }

        private static GrantSchemaPrivilege[] MapPermissionToSchemaPrivilege(PermissionId permissionId)
        {
            return permissionId switch
            {
                PermissionId.r => new[] { GrantSchemaPrivilege.USAGE },
                PermissionId.a => new[] { GrantSchemaPrivilege.CREATE },
                PermissionId.m => new[] { GrantSchemaPrivilege.ALL },
                _ => throw new NotImplementedException(permissionId.ToString()),
            };
        }

        private enum GrantSchemaPrivilege
        {
            ALL,
            CREATE,
            USAGE,
        }
    }
}
