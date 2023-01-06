using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organizations.Organization.Data.Spaces;
using GiantTeam.Postgres;
using GiantTeam.UserManagement.Services;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organizations.Organization.Services
{
    public class CreateSpaceInput
    {
        [Required, StringLength(100)]
        public string DatabaseName { get; set; } = null!;

        [Required, StringLength(100)]
        public string Name { get; set; } = null!;

        [Required, StringLength(50), DatabaseName]
        public string SchemaName { get; set; } = null!;
    }

    public class CreateSpaceResult
    {
        public string SpaceId { get; set; } = null!;
    }

    public class CreateSpaceService
    {
        private readonly ILogger<CreateSpaceService> logger;
        private readonly ValidationService validationService;
        private readonly UserDataFactory organizationDataFactory;

        public CreateSpaceService(
            ILogger<CreateSpaceService> logger,
            ValidationService validationService,
            UserDataFactory organizationDataFactory)
        {
            this.logger = logger;
            this.validationService = validationService;
            this.organizationDataFactory = organizationDataFactory;
        }

        public async Task<CreateSpaceResult> CreateSpaceAsync(CreateSpaceInput input)
        {
            try
            {
                return await ProcessAsync(input);
            }
            catch (Exception exception) when (exception.GetBaseException() is PostgresException ex)
            {
                logger.LogWarning(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                throw new ValidationException($"An error occurred that prevented creation of the \"{input.Name}\" space. {ex.MessageText.TrimEnd('.')}. {ex.Detail}", ex);
            }
        }

        private async Task<CreateSpaceResult> ProcessAsync(CreateSpaceInput input)
        {
            validationService.Validate(input);

            var dataService = organizationDataFactory.NewDataService(input.DatabaseName);
            using var elevatedDb = organizationDataFactory.NewDbContext(input.DatabaseName);
            await using var tx = await elevatedDb.Database.BeginTransactionAsync();

            var space = new Space()
            {
                SpaceId = input.SchemaName,
                Name = input.Name,
                SchemaName = input.SchemaName,
                Created = DateTime.UtcNow,
            };

            validationService.Validate(space);
            elevatedDb.Spaces.Spaces.Add(space);
            await elevatedDb.SaveChangesAsync();

            string schemaName = input.SchemaName;

            // Create the SCHEMA as the pg_database_owner.
            await dataService.ExecuteAsync(
                $"SET ROLE pg_database_owner",
                $"CREATE SCHEMA {Sql.Identifier(schemaName)}");

            // Commit the new Space record
            await tx.CommitAsync();

            return new()
            {
                SpaceId = space.SpaceId,
            };
        }
    }
}
