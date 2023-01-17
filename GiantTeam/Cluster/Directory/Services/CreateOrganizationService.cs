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
        public string OrganizationId { get; set; } = null!;
    }

    public class CreateOrganizationService
    {
        private readonly ILogger<CreateOrganizationService> logger;
        private readonly ValidationService validationService;
        private readonly SecurityDataService securityDataService;
        private readonly IDbContextFactory<ManagerDirectoryDbContext> managerDirectoryDbContextFactory;
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
            IDbContextFactory<ManagerDirectoryDbContext> managerDirectoryDbContextFactory,
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
            this.managerDirectoryDbContextFactory = managerDirectoryDbContextFactory;
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

        private async Task<CreateOrganizationResult> ProcessAsync(CreateOrganizationInput input)
        {
            if (!sessionService.User.Elevated || sessionService.User.DbElevatedUser is null)
            {
                throw new UnauthorizedException("Elevated rights are required to create an organization. Please login with elevated rights.");
            }

            validationService.Validate(input);

            var owner = new OrganizationRole()
            {
                OrganizationRoleId = Guid.NewGuid(),
                Name = "Owner",
                Created = DateTime.UtcNow,
            };
            var organization = new Data.Organization()
            {
                OrganizationId = Guid.NewGuid().ToString(),
                Name = input.Name,
                DatabaseName = input.DatabaseName,
                DatabaseOwnerOrganizationRoleId = owner.OrganizationRoleId,
                Created = DateTime.UtcNow,
                Roles = new(new[] { owner }),
            };
            await using (var managerDirectoryDb = await managerDirectoryDbContextFactory.CreateDbContextAsync())
            {
                validationService.ValidateAll(organization, owner);
                managerDirectoryDb.Organizations.Add(organization);
                await managerDirectoryDb.SaveChangesAsync();

                string databaseName = organization.DatabaseName;

                var elevatedDirectoryDataService = userDirectoryDataServiceFactory.NewElevatedDataService();
                try
                {
                    // Create the organization's initial roles.
                    // CREATEDB will be allowed until the organization database is created.
                    await securityDataService.ExecuteAsync(
                        $"CREATE ROLE {Sql.Identifier(owner.DbRole)} WITH INHERIT CREATEDB NOLOGIN NOSUPERUSER NOCREATEROLE NOREPLICATION ROLE {Sql.Identifier(sessionService.User.DbElevatedUser)}"
                    );

                    // Create the database as the new owner.
                    await elevatedDirectoryDataService.ExecuteAsync(
                        $"SET ROLE {Sql.Identifier(owner.DbRole)}",
                        $"CREATE DATABASE {Sql.Identifier(databaseName)}");

                    // Remove the CREATEDB privilege.
                    await securityDataService.ExecuteAsync($"ALTER ROLE {Sql.Identifier(owner.DbRole)} NOCREATEDB");

                    // Connect to the new database with elevated rights
                    var elevatedDatabaseService = userDataFactory.NewElevatedDataService(databaseName);

                    // These actions must be performed as the database owner
                    await elevatedDatabaseService.ExecuteAsync(
                        Sql.Format($"SET ROLE {Sql.Identifier(owner.DbRole)}"),
                        Sql.Format($"GRANT ALL ON DATABASE {Sql.Identifier(databaseName)} TO pg_database_owner"),
                        Sql.Format($"REVOKE ALL ON DATABASE {Sql.Identifier(databaseName)} FROM public"),
                        Sql.Format($"DROP SCHEMA IF EXISTS public CASCADE")
                    );

                    // Run the standard organization initialization script
                    await elevatedDatabaseService.ExecuteUnsanitizedAsync(OrganizationResources.ScriptOrganizationObjectsSql);

                    // Set the name of the root inode to match
                    // the name of the organization in the directory.
                    await using var elevatedDbContext = userDbContextFactory.NewElevatedDbContext<EtcDbContext>(organization.OrganizationId);
                    await elevatedDbContext.Inodes
                        .Where(o => o.InodeId == InodeId.Root)
                        .ExecuteUpdateAsync(o => o.SetProperty(p => p.Name, input.Name));
                }
                catch (Exception)
                {
                    try
                    {
                        await managerDirectoryDb
                            .Organizations
                            .Where(o => o.OrganizationId == organization.OrganizationId)
                            .ExecuteDeleteAsync();
                    }
                    catch (Exception cleanupException)
                    {
                        logger.LogWarning(cleanupException, "Cleanup failure following {DatabaseName} creation attempt", databaseName);
                    }

                    try
                    {
                        await elevatedDirectoryDataService.ExecuteAsync(
                            $"SET ROLE {Sql.Identifier(owner.DbRole)}",
                            $"DROP DATABASE IF EXISTS {Sql.Identifier(databaseName)}");
                    }
                    catch (Exception cleanupException)
                    {
                        logger.LogWarning(cleanupException, "Cleanup failure following {DatabaseName} creation attempt", databaseName);
                    }

                    try
                    {
                        await securityDataService.ExecuteAsync(
                            $"DROP ROLE IF EXISTS {Sql.Identifier(owner.DbRole)}");
                    }
                    catch (Exception cleanupException)
                    {
                        logger.LogWarning(cleanupException, "Cleanup failure following {DatabaseName} creation attempt", databaseName);
                    }

                    throw;
                }
            } // These connections must be closed so that subsequent transactions can be opened to create roles, etc.

            // Create non-elevated organization roles
            var admin = await createOrganizationRoleService.CreateOrganizationRoleAsync(new()
            {
                OrganizationId = organization.OrganizationId,
                RoleName = "Admin",
                MemberDbRoles = new List<string>() { sessionService.User.DbUser },
            });
            var member = await createOrganizationRoleService.CreateOrganizationRoleAsync(new()
            {
                OrganizationId = organization.OrganizationId,
                RoleName = "Member",
                MemberDbRoles = new List<string>() { sessionService.User.DbUser },
            });

            //// Grant access to the etc space
            //await grantSpaceService.GrantSpaceAsync(new()
            //{
            //    OrganizationId = organization.OrganizationId,
            //    InodeId = InodeId.Etc,
            //    AccessControlList = new()
            //    {
            //        new()
            //        {
            //            DbRole = admin.DbRole,
            //            Permissions = new[] { PermissionId.Read },
            //        },
            //        new()
            //        {
            //            DbRole = admin.DbRole,
            //            Permissions = new[] { PermissionId.Read },
            //        },
            //    },
            //});
            //// Grant access to the etc space
            //await grantSpaceService.GrantSpaceAsync(new()
            //{
            //    OrganizationId = organization.OrganizationId,
            //    InodeId = InodeId.Etc,
            //    AccessControlList = new()
            //    {
            //        new()
            //        {
            //            DbRole = admin.DbRole,
            //            Permissions = new[] { PermissionId.Read },
            //        },
            //        new()
            //        {
            //            DbRole = admin.DbRole,
            //            Permissions = new[] { PermissionId.Read },
            //        },
            //    },
            //});
            //foreach (var tableName in new[] { "inode", "file" })
            //{
            //    await grantTableService.GrantTableAsync(new()
            //    {
            //        OrganizationId = organization.OrganizationId,
            //        SpaceName = "etc",
            //        TableName = tableName,
            //        Grants = new()
            //        {
            //            new()
            //            {
            //                OrganizationRoleId = admin.OrganizationRoleId,
            //                Privileges = new[] { GrantTableInputPrivilege.SELECT },
            //            },
            //            new()
            //            {
            //                OrganizationRoleId = member.OrganizationRoleId,
            //                Privileges = new[] { GrantTableInputPrivilege.SELECT },
            //            },
            //        },
            //    });
            //}

            // Create the default Home space
            await createSpaceService.CreateSpaceAsync(new()
            {
                OrganizationId = organization.OrganizationId,
                SpaceName = "Home",
                AccessControlList = new Organization.Etc.Models.InodeAccess[]
                {
                    new()
                    {
                        DbRole = admin.DbRole,
                        Permissions = "ra".ToCharArray(),
                    },
                    new()
                    {
                        DbRole = member.DbRole,
                        Permissions = "r".ToCharArray(),
                    },
                },
            });

            return new()
            {
                OrganizationId = organization.OrganizationId,
            };
        }
    }
}
