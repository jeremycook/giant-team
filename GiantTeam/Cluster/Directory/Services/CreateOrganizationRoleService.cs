using GiantTeam.Cluster.Directory.Data;
using GiantTeam.Cluster.Directory.Helpers;
using GiantTeam.Cluster.Security.Services;
using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organization.Etc.Data;
using GiantTeam.Organization.Etc.Models;
using GiantTeam.Postgres;
using GiantTeam.UserData.Services;
using GiantTeam.UserManagement.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;

namespace GiantTeam.Cluster.Directory.Services
{
    public class CreateOrganizationRoleService
    {
        private readonly ILogger<CreateOrganizationRoleService> logger;
        private readonly ValidationService validationService;
        private readonly SecurityDataService securityDataService;
        private readonly IDbContextFactory<ManagerDirectoryDbContext> managerDirectoryDbContextFactory;
        private readonly UserDataServiceFactory userDataServiceFactory;
        private readonly SessionService sessionService;

        public CreateOrganizationRoleService(
            ILogger<CreateOrganizationRoleService> logger,
            ValidationService validationService,
            SecurityDataService securityDataService,
            IDbContextFactory<ManagerDirectoryDbContext> managerDirectoryDbContextFactory,
            UserDataServiceFactory userDataServiceFactory,
            SessionService sessionService)

        {
            this.logger = logger;
            this.validationService = validationService;
            this.securityDataService = securityDataService;
            this.managerDirectoryDbContextFactory = managerDirectoryDbContextFactory;
            this.userDataServiceFactory = userDataServiceFactory;
            this.sessionService = sessionService;
        }

        public async Task<Role> CreateOrganizationRoleAsync(CreateOrganizationRoleInput input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            validationService.Validate(input);

            if (!sessionService.User.Elevated || sessionService.User.DbElevatedUser is null)
            {
                throw new UnauthorizedException("Elevated rights are required to create an organization role. Please login with elevated rights.");
            }

            Data.Organization organization;
            await using (var managerDirectoryDb = await managerDirectoryDbContextFactory.CreateDbContextAsync())
            {
                organization = await managerDirectoryDb.Organizations
                    .SingleOrDefaultAsync(r => r.OrganizationId == input.OrganizationId)
                    ?? throw new NotFoundException($"The \"{input.OrganizationId}\" organization was not found.");
            }

            var elevatedDataService = userDataServiceFactory.NewElevatedDataService(organization.OrganizationId);

            // This will throw if the user does not have permission
            // to connect to the organization's database.
            string ownerDbRole;
            try
            {
                ownerDbRole = await elevatedDataService
                    .ScalarAsync($"""
SELECT r.rolname
FROM pg_catalog.pg_database d
JOIN pg_catalog.pg_roles r ON r.oid = d.datdba
WHERE datname = current_database()
""") as string ?? throw new InvalidOperationException($"Failed to determine the owner of the \"{organization.DatabaseName}\" database.");
            }
            catch (DbException ex)
            {
                logger.LogError(ex, "The {Sub} does not have permission to connect to the {OrganizationId} organization database.", sessionService.User.Sub, input.OrganizationId);
                throw new UnauthorizedException($"You do not have permission to create an organization role for the \"{input.OrganizationId}\" organization.");
            }

            if (organization.DbOwnerRole != ownerDbRole)
                throw new InvalidOperationException($"This is a bug. The owner of the \"{organization.DatabaseName}\" database, and the \"{organization.OrganizationId}\" organization owner role do not match.");

            try
            {
                // This will throw if the user is not a member of
                // pg_database_owner and the organization's owner DbRole
                await elevatedDataService.ExecuteAsync(
                    $"SET ROLE {Sql.Identifier(ownerDbRole)}",
                    $"SET ROLE pg_database_owner");
            }
            catch (DbException ex)
            {
                logger.LogError(ex, "The {Sub} does not have permission to create an organization role for the {OrganizationId} organization.", sessionService.User.Sub, input.OrganizationId);
                throw new UnauthorizedException($"You do not have permission to create an organization role for the \"{input.OrganizationId}\" organization.");
            }

            // Now we know that the user is allowed to create roles for this organization,
            // we can create the new DB role, and Root inode access record.
            var newRole = new RoleRecord(DateTime.UtcNow)
            {
                RoleId = DirectoryHelpers.OrganizationRole(Guid.NewGuid()),
                Name = input.RoleName,
            };
            var newAccess = new InodeAccessRecord(DateTime.UtcNow)
            {
                InodeId = InodeId.Root,
                RoleId = newRole.RoleId,
                Permissions = new[] { PermissionId.r },
            };
            validationService.ValidateAll(newRole, newAccess);

            await securityDataService.ExecuteAsync(
                $"CREATE ROLE {Sql.Identifier(newRole.RoleId)} WITH INHERIT NOCREATEDB NOLOGIN NOSUPERUSER NOCREATEROLE NOREPLICATION ROLE {Sql.IdentifierList(input.MemberDbRoles)}");

            try
            {
                // Grant connect to database and read root node
                // Execution order matters:
                //      The role table has a database connect privilege check
                //      The inode_access table references the role table
                await elevatedDataService.ExecuteAsync(
                    Sql.Format($"SET ROLE {Sql.Identifier(ownerDbRole)}"),
                    Sql.Format($"GRANT CONNECT ON DATABASE {Sql.Identifier(organization.DatabaseName)} TO {Sql.Identifier(newRole.RoleId)}"),
                    Sql.Insert(newRole),
                    Sql.Insert(newAccess));
            }
            catch (Exception)
            {
                try
                {
                    await securityDataService.ExecuteAsync(
                        $"DROP ROLE IF EXISTS {Sql.Identifier(newRole.RoleId)}");
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex, "Role creation rollback failure.");
                }

                throw;
            }

            return Role.CreateFrom(newRole);
        }
    }

    public class CreateOrganizationRoleInput
    {
        [RequiredGuid]
        public Guid OrganizationId { get; set; }

        [Required, StringLength(50), InodeName]
        public string RoleName { get; set; } = null!;

        [Required, MinLength(1)]
        public List<string> MemberDbRoles { get; set; } = null!;
    }
}
