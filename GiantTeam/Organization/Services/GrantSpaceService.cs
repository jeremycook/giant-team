using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Linq;
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
        public Etc.Models.InodeAccess[] AccessControlList { get; set; } = null!;
    }

    public class GrantSpaceResult
    {
    }

    public class GrantSpaceService
    {
        private readonly ILogger<GrantSpaceService> logger;
        private readonly ValidationService validationService;
        private readonly FetchInodeService fetchInodeService;
        private readonly UserDataServiceFactory userDataServiceFactory;

        public GrantSpaceService(
            ILogger<GrantSpaceService> logger,
            ValidationService validationService,
            FetchInodeService fetchInodeService,
            UserDataServiceFactory userDataServiceFactory)
        {
            this.logger = logger;
            this.validationService = validationService;
            this.fetchInodeService = fetchInodeService;
            this.userDataServiceFactory = userDataServiceFactory;
        }

        public async Task<GrantSpaceResult> GrantSpaceAsync(GrantSpaceInput input)
        {
            validationService.Validate(input);

            var space = await fetchInodeService.FetchInodeAsync(input.OrganizationId, input.InodeId);
            if (space.InodeTypeId != InodeTypeId.Space)
            {
                throw new InvalidOperationException("The inode is not a space.");
            }

            var result = await GrantSpaceAsync(input.OrganizationId, space.InodeId, space.UglyName, input.AccessControlList);

            return result;
        }

        public async Task<GrantSpaceResult> GrantSpaceAsync(
            Guid organizationId,
            Guid inodeId,
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
INSERT INTO etc.inode_access (inode_id, db_role, permissions) VALUES ({inodeId}, {access.DbRole}, {access.Permissions})
    ON CONFLICT ON CONSTRAINT inode_access_pkey
    DO UPDATE SET permissions = {access.Permissions}
"""));
                commands.Add(Sql.Format($"REVOKE ALL ON SCHEMA {Sql.Identifier(schemaName)} FROM {Sql.Identifier(access.DbRole)}"));

                var schemaPrivileges = access.Permissions
                    .SelectMany(permission => SchemaPermissionId.Map.TryGetValue(permission, out var value) ? value : Array.Empty<string>())
                    .Distinct()
                    .ToArray();

                if (schemaPrivileges.Any())
                {
                    commands.Add(Sql.Format($"GRANT {Sql.Raw(schemaPrivileges.Join(','))} ON SCHEMA {Sql.Identifier(schemaName)} TO {Sql.Identifier(access.DbRole)}"));
                }
            }

            await elevatedDataService.ExecuteAsync(commands);

            return new()
            {
            };
        }
    }
}
