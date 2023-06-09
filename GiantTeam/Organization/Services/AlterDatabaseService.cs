﻿using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.DatabaseDefinition.Alterations.Models;
using GiantTeam.Postgres;
using GiantTeam.UserData.Services;
using GiantTeam.UserManagement.Services;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Services
{
    public class AlterDatabaseInput
    {
        [RequiredGuid]
        public Guid OrganizationId { get; set; }

        [Required, MinLength(1)]
        public DatabaseAlteration[] Changes { get; set; } = null!;
    }

    public class AlterDatabaseOutput
    {
    }

    public class AlterDatabaseService
    {
        private readonly ILogger<AlterDatabaseService> logger;
        private readonly ValidationService validationService;
        private readonly SessionService sessionService;
        private readonly UserDataServiceFactory userDataFactory;

        public AlterDatabaseService(
            ILogger<AlterDatabaseService> logger,
            ValidationService validationService,
            SessionService sessionService,
            UserDataServiceFactory userDataFactory)
        {
            this.logger = logger;
            this.validationService = validationService;
            this.sessionService = sessionService;
            this.userDataFactory = userDataFactory;
        }

        public async Task<AlterDatabaseOutput> ChangeDatabaseAsync(AlterDatabaseInput input)
        {
            return await ProcessAsync(input);
        }

        private async Task<AlterDatabaseOutput> ProcessAsync(AlterDatabaseInput input)
        {
            validationService.Validate(input);

            string migrationScript =
                //$"SET ROLE pg_database_owner;\n" +
                PgDatabaseScripter.Singleton.ScriptAlterations(input.Changes);

            logger.LogInformation("Executing change database script as {UserId}: {CommandText}",
                sessionService.User.UserId,
                migrationScript);

            var dataService = userDataFactory.NewElevatedDataService(input.OrganizationId);
            try
            {
                await dataService.ExecuteUnsanitizedAsync(migrationScript);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "{ExceptionType}: {ExceptionMessage} when executing {CommandText}",
                    ex.GetBaseException().GetType(),
                    ex.GetBaseException().Message,
                    migrationScript);

                if (ex.GetBaseException() is PostgresException postgresException)
                {
                    string message = $"Database error: {postgresException.MessageText.TrimEnd('.')}.";

                    if (postgresException.Detail is not null)
                    {
                        message += $" Detail: {postgresException.Detail.TrimEnd('.')}.";
                    }

                    if (postgresException.Hint is not null)
                    {
                        message += $" Hint: {postgresException.Hint.TrimEnd('.')}.";
                    }

                    message += $" SQL:\n{migrationScript}";

                    throw new ValidationException(message);
                }

                throw;
            }

            return new AlterDatabaseOutput()
            {
            };
        }
    }
}
