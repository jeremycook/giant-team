using GiantTeam.Cluster.Directory.Data;
using GiantTeam.Cluster.Security.Services;
using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organization.Services;
using GiantTeam.Postgres;
using GiantTeam.UserData.Services;
using GiantTeam.UserManagement.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;
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

        public async Task<CreateOrganizationRoleResult> CreateOrganizationRoleAsync(CreateOrganizationRoleInput input)
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

            await using var managerDirectoryDb = await managerDirectoryDbContextFactory.CreateDbContextAsync();
            await using var tx = await managerDirectoryDb.Database.BeginTransactionAsync();

            var organization = await managerDirectoryDb
                .Organizations
                .Include(o => o.Roles)
                .SingleOrDefaultAsync(r => r.OrganizationId == input.OrganizationId)
                ?? throw new NotFoundException($"The \"{input.OrganizationId}\" organization was not found.");

            var elevatedDataService = userDataServiceFactory.NewElevatedDataService(organization.DatabaseName);

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

            if (!organization.Roles!.Any(o => o.DbRole == ownerDbRole && o.OrganizationRoleId == organization.DatabaseOwnerOrganizationRoleId))
                throw new InvalidOperationException($"The owner of the \"{organization.DatabaseName}\" database, and the \"{organization.OrganizationId}\" organization owner role \"{organization.DatabaseOwnerOrganizationRoleId}\" do not match.");

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
            // create the directory record and the new role with its members.
            var newRole = new OrganizationRole()
            {
                OrganizationId = organization.OrganizationId,
                OrganizationRoleId = Guid.NewGuid(),
                Name = input.RoleName,
                Created = DateTime.UtcNow,
            };
            try
            {
                validationService.Validate(newRole);
                managerDirectoryDb.OrganizationRoles.Add(newRole);
                await managerDirectoryDb.SaveChangesAsync();

                await securityDataService.ExecuteAsync(
                    $"CREATE ROLE {Sql.Identifier(newRole.DbRole)} WITH INHERIT NOCREATEDB NOLOGIN NOSUPERUSER NOCREATEROLE NOREPLICATION ROLE {Sql.IdentifierList(input.MemberDbRoles)}");

                // Grant connect to database
                await elevatedDataService.ExecuteAsync(new[]
                {
                    Sql.Format($"SET ROLE {Sql.Identifier(ownerDbRole)}"),
                    Sql.Format($"GRANT CONNECT ON DATABASE {Sql.Identifier(organization.DatabaseName)} TO {Sql.Identifier(newRole.DbRole)}"),
                });
            }
            catch (Exception)
            {
                try
                {
                    await securityDataService.ExecuteAsync(
                        $"DROP ROLE IF EXISTS {Sql.Identifier(newRole.DbRole)}");
                }
                catch (Exception cleanupException)
                {
                    logger.LogInformation(cleanupException, "Suppressed drop roles cleanup failure following error when creating {OrganizationRole} for {OrganizationId}.", input.RoleName, input.OrganizationId);
                }

                throw;
            }

            await tx.CommitAsync();

            return new()
            {
                OrganizationRoleId = newRole.OrganizationRoleId,
            };
        }
    }

    public class CreateOrganizationRoleInput
    {
        [Required, StringLength(50), DatabaseName]
        public string OrganizationId { get; set; } = null!;

        [Required, StringLength(50), InodeName]
        public string RoleName { get; set; } = null!;

        [Required, MinLength(1)]
        public List<string> MemberDbRoles { get; set; } = null!;
    }

    public class CreateOrganizationRoleResult
    {
        public Guid OrganizationRoleId { get; set; }
    }
}
