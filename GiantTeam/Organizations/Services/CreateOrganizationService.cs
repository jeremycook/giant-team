using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organizations.Directory.Data;
using GiantTeam.Organizations.Organization.Resources;
using GiantTeam.Postgres;
using GiantTeam.UserManagement.Services;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organizations.Services
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
        private readonly DirectoryDataService directoryDataService;
        private readonly DirectoryManagerDbContext directoryManagerDb;
        private readonly SessionService sessionService;

        public CreateOrganizationService(
            ILogger<CreateOrganizationService> logger,
            ValidationService validationService,
            SecurityDataService securityDataService,
            DirectoryDataService directoryUserService,
            DirectoryManagerDbContext directoryManagerDb,
            SessionService sessionService)
        {
            this.logger = logger;
            this.validationService = validationService;
            this.securityDataService = securityDataService;
            this.directoryDataService = directoryUserService;
            this.directoryManagerDb = directoryManagerDb;
            this.sessionService = sessionService;
        }

        public async Task<CreateOrganizationResult> CreateOrganizationAsync(CreateOrganizationInput input)
        {
            try
            {
                return await ProcessAsync(input);
            }
            catch (Exception exception) when (exception.GetBaseException() is PostgresException ex)
            {
                logger.LogWarning(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                throw new ValidationException($"An error occurred that prevented creation of the \"{input.Name}\" organization. {ex.MessageText.TrimEnd('.')}. {ex.Detail}", ex);
            }
        }

        private async Task<CreateOrganizationResult> ProcessAsync(CreateOrganizationInput input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            validationService.Validate(input);

            await using var dmtx = await directoryManagerDb.Database.BeginTransactionAsync();

            var owner = new OrganizationRole() { Name = "Owner" }.Init();
            var member = new OrganizationRole() { Name = "Member" }.Init();
            var guest = new OrganizationRole() { Name = "Guest" }.Init();
            var org = new Directory.Data.Organization()
            {
                OrganizationId = input.DatabaseName,
                Name = input.Name,
                DatabaseName = input.DatabaseName,
            }.Init();
            org.Roles!.AddRange(new[]
            {
                owner,
                member,
                guest,
            });

            validationService.Validate(org);
            directoryManagerDb.Organizations.Add(org);
            await directoryManagerDb.SaveChangesAsync();

            string databaseName = org.DatabaseName;

            try
            {
                // Create the organization's initial roles.
                // CREATEDB will be allowed until the organization database is created.
                await securityDataService.ExecuteAsync(
                    $"CREATE ROLE {Sql.Identifier(owner.DbRole)} WITH INHERIT CREATEDB NOLOGIN NOSUPERUSER NOCREATEROLE NOREPLICATION ROLE {Sql.Identifier(sessionService.User.DbElevated)}",
                    $"CREATE ROLE {Sql.Identifier(member.DbRole)} WITH INHERIT NOCREATEDB NOLOGIN NOSUPERUSER NOCREATEROLE NOREPLICATION ROLE {Sql.Identifier(sessionService.User.DbRegular)}",
                    $"CREATE ROLE {Sql.Identifier(guest.DbRole)} WITH INHERIT NOCREATEDB NOLOGIN NOSUPERUSER NOCREATEROLE NOREPLICATION");

                // Create the database as the new owner.
                await directoryDataService.ExecuteAsync(
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
                var databaseDataService = directoryDataService.CloneDataService(databaseName);
                await databaseDataService.ExecuteAsync(batch);
                await databaseDataService.ExecuteUnsanitizedAsync(OrganizationResources.SpacesSql);
            }
            catch (Exception ex)
            {
                try
                {
                    await directoryDataService.ExecuteAsync(
                        $"SET ROLE {Sql.Identifier(owner.DbRole)}",
                        $"DROP DATABASE IF EXISTS {Sql.Identifier(databaseName)}");

                    await securityDataService.ExecuteAsync(
                        $"DROP ROLE IF EXISTS {Sql.Identifier(owner.DbRole)}",
                        $"DROP ROLE IF EXISTS {Sql.Identifier(member.DbRole)}",
                        $"DROP ROLE IF EXISTS {Sql.Identifier(guest.DbRole)}");
                }
                catch (Exception cleanupEx)
                {
                    var aggregateException = new AggregateException(cleanupEx, ex);
                    logger.LogError(aggregateException, "Suppressed cleanup failure following error when creating {DatabaseName}.", databaseName);
                }

                throw;
            }

            await dmtx.CommitAsync();

            return new()
            {
                OrganizationId = org.OrganizationId,
            };
        }
    }
}
