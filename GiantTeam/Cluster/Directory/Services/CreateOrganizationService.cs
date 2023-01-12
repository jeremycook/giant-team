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
        private readonly ManagerDirectoryDbContext managerDirectoryDb;
        private readonly UserDataServiceFactory userDataFactory;
        private readonly UserDbContextFactory userDbContextFactory;
        private readonly SessionService sessionService;
        private readonly CreateSpaceService createSpaceService;

        public CreateOrganizationService(
            ILogger<CreateOrganizationService> logger,
            ValidationService validationService,
            SecurityDataService securityDataService,
            ManagerDirectoryDbContext managerDirectoryDb,
            UserDataServiceFactory userDataFactory,
            UserDbContextFactory userDbContextFactory,
            SessionService sessionService,
            CreateSpaceService createSpaceService)
        {
            this.logger = logger;
            this.validationService = validationService;
            this.securityDataService = securityDataService;
            this.managerDirectoryDb = managerDirectoryDb;
            this.userDataFactory = userDataFactory;
            this.userDbContextFactory = userDbContextFactory;
            this.sessionService = sessionService;
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
            if (sessionService.User.DbElevatedUser is null)
            {
                throw new UnauthorizedException("Elevated rights are required to create an organization. Please login with elevated rights.");
            }

            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            validationService.Validate(input);

            await using var dmtx = await managerDirectoryDb.Database.BeginTransactionAsync();

            var owner = new OrganizationRole() { Name = "Owner", Created = DateTime.UtcNow };
            var member = new OrganizationRole() { Name = "Member", Created = DateTime.UtcNow };
            var guest = new OrganizationRole() { Name = "Guest", Created = DateTime.UtcNow };
            var organization = new Data.Organization()
            {
                OrganizationId = input.DatabaseName,
                Name = input.Name,
                DatabaseName = input.DatabaseName,
                Created = DateTime.UtcNow,
                Roles = new(new[]
                {
                    owner,
                    member,
                    guest,
                }),
            };

            validationService.Validate(organization);
            managerDirectoryDb.Organizations.Add(organization);
            await managerDirectoryDb.SaveChangesAsync();

            string databaseName = organization.DatabaseName;

            var elevatedDirectoryDataService = userDataFactory.NewElevatedDataService(DirectoryHelpers.Database);
            try
            {
                // Create the organization's initial roles.
                // CREATEDB will be allowed until the organization database is created.
                await securityDataService.ExecuteAsync(
                    $"CREATE ROLE {Sql.Identifier(owner.DbRole)} WITH INHERIT CREATEDB NOLOGIN NOSUPERUSER NOCREATEROLE NOREPLICATION ROLE {Sql.Identifier(sessionService.User.DbElevatedUser)}",
                    $"CREATE ROLE {Sql.Identifier(member.DbRole)} WITH INHERIT NOCREATEDB NOLOGIN NOSUPERUSER NOCREATEROLE NOREPLICATION ROLE {Sql.Identifier(sessionService.User.DbUser)}",
                    $"CREATE ROLE {Sql.Identifier(guest.DbRole)} WITH INHERIT NOCREATEDB NOLOGIN NOSUPERUSER NOCREATEROLE NOREPLICATION"
                );

                // Create the database as the new owner.
                await elevatedDirectoryDataService.ExecuteAsync(
                    $"SET ROLE {Sql.Identifier(owner.DbRole)}",
                    $"CREATE DATABASE {Sql.Identifier(databaseName)}");

                // Remove the CREATEDB privilege.
                await securityDataService.ExecuteAsync($"ALTER ROLE {Sql.Identifier(owner.DbRole)} NOCREATEDB");

                // Connect to the new database and initialize it.
                await using var batch = new NpgsqlBatch()
                {
                    BatchCommands =
                    {
                        // Perform these actions as the database owner
                        Sql.Format($"SET ROLE {Sql.Identifier(owner.DbRole)}"),
                        Sql.Format($"DROP SCHEMA IF EXISTS public CASCADE"),
                        Sql.Format($"GRANT ALL ON DATABASE {Sql.Identifier(databaseName)} TO pg_database_owner"),
                        Sql.Format($"GRANT CONNECT, TEMPORARY ON DATABASE {Sql.Identifier(databaseName)} TO {Sql.Identifier(member.DbRole)}"),
                        Sql.Format($"GRANT CONNECT ON DATABASE {Sql.Identifier(databaseName)} TO {Sql.Identifier(guest.DbRole)}"),

                        // Switch to pg_database_owner
                        Sql.Format($"SET ROLE pg_database_owner"),
                        Sql.Format($"REVOKE ALL ON DATABASE {Sql.Identifier(databaseName)} FROM public"),
                        Sql.Format($"GRANT CONNECT ON DATABASE {Sql.Identifier(databaseName)} TO {Sql.Identifier(sessionService.User.DbUser)}"),
                    }
                };
                var elevatedDatabaseService = userDataFactory.NewElevatedDataService(databaseName);
                await elevatedDatabaseService.ExecuteAsync(batch);
#pragma warning disable CS0618 // Type or member is obsolete
                await elevatedDatabaseService.ExecuteUnsanitizedAsync(OrganizationResources.ScriptOrganizationObjectsSql);
#pragma warning restore CS0618 // Type or member is obsolete

                // Set the name of the root node to match
                // the name of the organization from the directory.
                await using var elevatedDbContext = userDbContextFactory.NewElevatedDbContext<EtcDbContext>(input.DatabaseName, "etc");
                var root = await elevatedDbContext.Nodes
                    .SingleAsync(o => o.NodeId == NodeId.Root);
                root.Name = input.Name;
                await elevatedDbContext.SaveChangesAsync();

                // Create the default Home space
                await createSpaceService.CreateSpaceAsync(new()
                {
                    DatabaseName = organization.DatabaseName,
                    Name = "Home",
                });
            }
            catch (Exception)
            {
                try
                {
                    await elevatedDirectoryDataService.ExecuteAsync(
                        $"SET ROLE {Sql.Identifier(owner.DbRole)}",
                        $"DROP DATABASE IF EXISTS {Sql.Identifier(databaseName)}");
                }
                catch (Exception cleanupException)
                {
                    logger.LogInformation(cleanupException, "Suppressed drop database cleanup failure following error when creating {DatabaseName}.", databaseName);
                }

                try
                {
                    await securityDataService.ExecuteAsync(
                        $"DROP ROLE IF EXISTS {Sql.Identifier(owner.DbRole)}",
                        $"DROP ROLE IF EXISTS {Sql.Identifier(member.DbRole)}",
                        $"DROP ROLE IF EXISTS {Sql.Identifier(guest.DbRole)}");
                }
                catch (Exception cleanupException)
                {
                    logger.LogInformation(cleanupException, "Suppressed drop roles cleanup failure following error when creating {DatabaseName}.", databaseName);
                }

                throw;
            }

            await dmtx.CommitAsync();

            return new()
            {
                OrganizationId = organization.OrganizationId,
            };
        }
    }
}
