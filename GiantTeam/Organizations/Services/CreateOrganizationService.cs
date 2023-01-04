using GiantTeam.ComponentModel.Services;
using GiantTeam.Organizations.Organization.Resources;
using GiantTeam.Organizations.Organization.Services;
using GiantTeam.Postgres;
using GiantTeam.UserManagement.Services;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organizations.Services
{
    public class CreateOrganizationProps
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = null!;

        public bool IsPublic { get; set; }
    }

    public class CreateOrganizationResult
    {
        public string Id { get; set; } = null!;
    }

    public class CreateOrganizationService
    {
        private readonly ILogger<CreateOrganizationService> logger;
        private readonly ValidationService validationService;
        private readonly SecurityDataService securityDataService;
        private readonly OrganizationDataService organizationDataService;
        private readonly DirectoryDataService directoryDataService;
        private readonly SessionService sessionService;

        public CreateOrganizationService(
            ILogger<CreateOrganizationService> logger,
            ValidationService validationService,
            SecurityDataService securityDataService,
            OrganizationDataService organizationDataService,
            DirectoryDataService directoryDataService,
            SessionService sessionService)
        {
            this.logger = logger;
            this.validationService = validationService;
            this.securityDataService = securityDataService;
            this.organizationDataService = organizationDataService;
            this.directoryDataService = directoryDataService;
            this.sessionService = sessionService;
        }

        public async Task<CreateOrganizationResult> CreateOrganizationAsync(CreateOrganizationProps props)
        {
            try
            {
                return await ProcessAsync(props);
            }
            catch (Exception exception) when (exception.GetBaseException() is PostgresException ex)
            {
                logger.LogWarning(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                throw new ValidationException($"An error occurred that prevented the \"{props.Name}\" organization from being created. {ex.MessageText.TrimEnd('.')}. {ex.Detail}", ex);
            }
        }

        private async Task<CreateOrganizationResult> ProcessAsync(CreateOrganizationProps props)
        {
            if (props is null)
            {
                throw new ArgumentNullException(nameof(props));
            }

            validationService.Validate(props);

            string databaseName = props.Name!;
            string databaseOwner = $"{databaseName}:owner";
            string databaseUser = $"{databaseName}:user";

            try
            {
                // The organization owner must be created before connecting to the info database.
                // CREATEDB will be allowed until the organization database is created.
                await securityDataService.ExecuteAsync(
                    $"CREATE ROLE {Sql.Identifier(databaseOwner)} WITH INHERIT CREATEDB NOLOGIN NOSUPERUSER NOCREATEROLE NOREPLICATION ROLE {Sql.Identifier(sessionService.User.DbAdmin)}",
                    $"CREATE ROLE {Sql.Identifier(databaseUser)} WITH INHERIT NOCREATEDB NOLOGIN NOSUPERUSER NOCREATEROLE NOREPLICATION ROLE {Sql.Identifier(sessionService.User.DbRole)}");

                // Create the database
                await directoryDataService.ExecuteAsync(
                    $"SET ROLE {Sql.Identifier(databaseOwner)}",
                    $"CREATE DATABASE {Sql.Identifier(databaseName)}");

                // Connect to the info database as the new organization owner.
                // Initialize the new organization database.
                await using var batch = new NpgsqlBatch()
                {
                    BatchCommands =
                    {
                        // Perform these actions as the database owner
                        Sql.Format($"SET ROLE {Sql.Identifier(databaseOwner)}"),
                        Sql.Format($"DROP SCHEMA IF EXISTS public CASCADE"),
                        Sql.Format($"GRANT ALL ON DATABASE {Sql.Identifier(databaseName)} TO pg_database_owner"),

                        // Switch to pg_database_owner
                        Sql.Format($"SET ROLE pg_database_owner"),
                        Sql.Format($"REVOKE ALL ON DATABASE {Sql.Identifier(databaseName)} FROM public"),
                        // TODO: I'm hoping this won't work.
                        Sql.Raw(OrganizationResources.SpacesSql),
                    }
                };
                if (props.IsPublic)
                {
                    batch.BatchCommands.Add(Sql.Format($"GRANT CONNECT ON DATABASE {Sql.Identifier(databaseName)} TO public;"));
                }

                await organizationDataService.ExecuteAsync(batch);
            }
            catch (Exception)
            {
                try
                {
                    await directoryDataService.ExecuteAsync(
                        $"SET ROLE {Sql.Identifier(databaseOwner)}",
                        $"DROP DATABASE IF EXISTS {Sql.Identifier(databaseName)}");

                    await securityDataService.ExecuteAsync(
                        $"DROP ROLE IF EXISTS {Sql.Identifier(databaseOwner)}",
                        $"DROP ROLE IF EXISTS {Sql.Identifier(databaseUser)}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Suppressed cleanup failure {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                }

                throw;
            }

            // Remove the CREATEDB privilege.
            await securityDataService.ExecuteAsync($"ALTER ROLE {Sql.Identifier(databaseOwner)} NOCREATEDB");

            return new()
            {
            };
        }
    }
}
