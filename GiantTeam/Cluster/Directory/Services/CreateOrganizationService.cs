using GiantTeam.Cluster.Directory.Data;
using GiantTeam.Cluster.Directory.Helpers;
using GiantTeam.Cluster.Security.Services;
using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organization.Etc.Data;
using GiantTeam.Organization.Etc.Models;
using GiantTeam.Organization.Resources;
using GiantTeam.Organization.Services;
using GiantTeam.Postgres;
using GiantTeam.UserData.Services;
using GiantTeam.UserManagement.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Cluster.Directory.Services
{
    public class CreateOrganizationInput
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = null!;

        [Required, StringLength(50), DatabaseName]
        public string DatabaseName { get; set; } = null!;
    }

    public class CreateOrganizationResult
    {
        public Guid OrganizationId { get; set; }
    }

    public class CreateOrganizationService
    {
        private readonly ILogger<CreateOrganizationService> logger;
        private readonly ValidationService validationService;
        private readonly SecurityDataService securityDataService;
        private readonly DirectoryManagementDataService directoryManagementDataService;
        private readonly UserDirectoryDataServiceFactory userDirectoryDataServiceFactory;
        private readonly UserDataServiceFactory userDataFactory;
        private readonly UserDbContextFactory userDbContextFactory;
        private readonly SessionService sessionService;
        private readonly CreateOrganizationRoleService createOrganizationRoleService;
        private readonly GrantSpaceService grantSpaceService;
        private readonly GrantTableService grantTableService;
        private readonly CreateSpaceService createSpaceService;

        public CreateOrganizationService(
            ILogger<CreateOrganizationService> logger,
            ValidationService validationService,
            SecurityDataService securityDataService,
            DirectoryManagementDataService directoryManagementDataService,
            UserDirectoryDataServiceFactory userDirectoryDataServiceFactory,
            UserDataServiceFactory userDataFactory,
            UserDbContextFactory userDbContextFactory,
            SessionService sessionService,
            CreateOrganizationRoleService createOrganizationRoleService,
            GrantSpaceService grantSpaceService,
            GrantTableService grantTableService,
            CreateSpaceService createSpaceService)
        {
            this.logger = logger;
            this.validationService = validationService;
            this.securityDataService = securityDataService;
            this.directoryManagementDataService = directoryManagementDataService;
            this.userDirectoryDataServiceFactory = userDirectoryDataServiceFactory;
            this.userDataFactory = userDataFactory;
            this.userDbContextFactory = userDbContextFactory;
            this.sessionService = sessionService;
            this.createOrganizationRoleService = createOrganizationRoleService;
            this.grantSpaceService = grantSpaceService;
            this.grantTableService = grantTableService;
            this.createSpaceService = createSpaceService;
        }

        public async Task<CreateOrganizationResult> CreateOrganizationAsync(CreateOrganizationInput input)
        {
            try
            {
                return await ProcessAsync(input);
            }
            catch (Exception exception) when (exception.GetBaseException() is PostgresException ex)
            {
                logger.LogError(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                throw new ValidationException($"An error occurred that prevented creation of the \"{input.Name}\" organization. {ex.MessageText.TrimEnd('.')}. {ex.Detail}", ex);
            }
        }

        enum Actions
        {
            InsertedOrganizationRecords,
            CreatedOwnerRole,
            CreatedDatabase,
            CreatedOrganizationRole,
        }
        private async Task<CreateOrganizationResult> ProcessAsync(CreateOrganizationInput input)
        {
            if (!sessionService.User.Elevated || sessionService.User.DbElevatedUser is null)
            {
                throw new UnauthorizedException("Elevated rights are required to create an organization. Please login with elevated rights.");
            }

            validationService.Validate(input);

            var organization = new Data.Organization()
            {
                OrganizationId = Guid.NewGuid(),
                Name = input.Name,
                DatabaseName = input.DatabaseName,
                DatabaseOwnerOrganizationRoleId = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                Roles = null,
            };
            var owner = new OrganizationRole()
            {
                OrganizationRoleId = organization.DatabaseOwnerOrganizationRoleId,
                OrganizationId = organization.OrganizationId,
                Name = "Owner",
                Created = DateTime.UtcNow,
                Description = null,
            };
            validationService.ValidateAll(organization, owner);

            var elevatedDirectoryDataService = userDirectoryDataServiceFactory.NewElevatedDataService();
            var changes = new List<(Actions Actions, object? Data)>();
            try
            {
                await directoryManagementDataService.ExecuteAsync(
                    Sql.Insert(organization),
                    Sql.Insert(owner));
                changes.Add((Actions.InsertedOrganizationRecords, new object[] { organization, owner }));

                // Create the organization's initial roles.
                // CREATEDB will be allowed until the organization database is created.
                await securityDataService.ExecuteAsync(
                    $"CREATE ROLE {Sql.Identifier(owner.DbRole)} WITH INHERIT CREATEDB NOLOGIN NOSUPERUSER NOCREATEROLE NOREPLICATION ROLE {Sql.Identifier(sessionService.User.DbElevatedUser)}");
                changes.Add((Actions.CreatedOwnerRole, owner.DbRole));

                // Create the database as the new owner.
                // Database creation cannot run in a transaction
                await elevatedDirectoryDataService.ExecuteAsync(
                    $"SET ROLE {Sql.Identifier(owner.DbRole)}",
                    $"CREATE DATABASE {Sql.Identifier(organization.DatabaseName)}");
                changes.Add((Actions.CreatedDatabase, organization.DatabaseName));

                // Remove the CREATEDB privilege.
                await securityDataService.ExecuteAsync($"ALTER ROLE {Sql.Identifier(owner.DbRole)} NOCREATEDB");

                // Now we can connect to the new database with elevated rights
                var elevatedDatabaseService = userDataFactory.NewElevatedDataService(organization.OrganizationId);

                // These actions must be performed as the database owner
                await elevatedDatabaseService.ExecuteAsync(
                    Sql.Format($"SET ROLE {Sql.Identifier(owner.DbRole)}"),
                    Sql.Format($"GRANT ALL ON DATABASE {Sql.Identifier(organization.DatabaseName)} TO pg_database_owner"),
                    Sql.Format($"REVOKE ALL ON DATABASE {Sql.Identifier(organization.DatabaseName)} FROM public"),
                    Sql.Format($"DROP SCHEMA IF EXISTS public CASCADE"));

                // Run the standard organization initialization script
                await elevatedDatabaseService.ExecuteUnsanitizedAsync(OrganizationResources.ScriptOrganizationObjectsSql);

                // Set the name of the root inode to match
                // the name of the organization in the directory.
                await elevatedDatabaseService.ExecuteAsync(
                    $"UPDATE etc.inode SET name = {input.Name} WHERE inode_id = {InodeId.Root}");

                // Create non-elevated organization roles
                var admin = await createOrganizationRoleService.CreateOrganizationRoleAsync(new()
                {
                    OrganizationId = organization.OrganizationId,
                    RoleName = "Admin",
                    MemberDbRoles = new List<string>() { sessionService.User.DbUser },
                });
                changes.Add((Actions.CreatedOrganizationRole, admin));

                var member = await createOrganizationRoleService.CreateOrganizationRoleAsync(new()
                {
                    OrganizationId = organization.OrganizationId,
                    RoleName = "Member",
                    MemberDbRoles = new List<string>() { sessionService.User.DbUser },
                });
                changes.Add((Actions.CreatedOrganizationRole, member));

                // Create the default Home space
                await createSpaceService.CreateSpaceAsync(new()
                {
                    OrganizationId = organization.OrganizationId,
                    SpaceName = "Home",
                    AccessControlList = new Organization.Etc.Models.InodeAccess[]
                    {
                        new () { DbRole = admin.DbRole, Permissions = "ra".ToCharArray() },
                        new () { DbRole = member.DbRole, Permissions = "r".ToCharArray() },
                    },
                });

                return new()
                {
                    OrganizationId = organization.OrganizationId,
                };
            }
            catch (Exception)
            {
                // Unwind changes in reverse order
                changes.Reverse();
                foreach (var change in changes)
                {
                    try
                    {
                        switch (change.Actions)
                        {
                            case Actions.InsertedOrganizationRecords:
                                await directoryManagementDataService.ExecuteAsync(
                                    Sql.Delete(owner),
                                    Sql.Delete(organization));
                                break;

                            case Actions.CreatedOwnerRole:
                                await securityDataService.ExecuteAsync(
                                    $"DROP ROLE IF EXISTS {Sql.Identifier(owner.DbRole)}");
                                break;

                            case Actions.CreatedDatabase:
                                await elevatedDirectoryDataService.ExecuteAsync(
                                    $"SET ROLE {Sql.Identifier(owner.DbRole)}",
                                    $"DROP DATABASE IF EXISTS {Sql.Identifier(organization.DatabaseName)}");
                                break;

                            case Actions.CreatedOrganizationRole when change.Data is CreateOrganizationRoleResult role:
                                var elevatedDatabaseService = userDataFactory.NewElevatedDataService(organization.OrganizationId);
                                await elevatedDatabaseService.ExecuteAsync(
                                    $"REVOKE CONNECT ON DATABASE {Sql.Identifier(organization.DatabaseName)} FROM {Sql.Identifier(role.DbRole)}");
                                await securityDataService.ExecuteAsync(
                                    $"DROP ROLE IF EXISTS {Sql.Identifier(role.DbRole)}");
                                await directoryManagementDataService.ExecuteAsync(
                                    $"DELETE FROM directory.organization_role WHERE organization_role_id = {role.OrganizationRoleId}");
                                break;

                            default:
                                throw new NotImplementedException(change.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to unwind {Change}", change.Actions);
                    }
                }

                throw;
            }
        }
    }
}
