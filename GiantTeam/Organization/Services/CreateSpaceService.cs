using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organization.Etc.Data;
using GiantTeam.Organization.Etc.Models;
using GiantTeam.Postgres;
using GiantTeam.UserData.Services;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Services
{
    public class CreateSpaceInput
    {
        [Required, StringLength(50)]
        public string DatabaseName { get; set; } = null!;

        [Required, StringLength(50), NodeName]
        public string Name { get; set; } = null!;
    }

    public class CreateSpaceResult
    {
        public Guid NodeId { get; set; }
    }

    public class CreateSpaceService
    {
        private readonly ILogger<CreateSpaceService> logger;
        private readonly ValidationService validationService;
        private readonly UserDbContextFactory userDbContextFactory;
        private readonly UserDataServiceFactory userDataServiceFactory;

        public CreateSpaceService(
            ILogger<CreateSpaceService> logger,
            ValidationService validationService,
            UserDbContextFactory userDbContextFactory,
            UserDataServiceFactory userDataServiceFactory)
        {
            this.logger = logger;
            this.validationService = validationService;
            this.userDbContextFactory = userDbContextFactory;
            this.userDataServiceFactory = userDataServiceFactory;
        }

        public async Task<CreateSpaceResult> CreateSpaceAsync(CreateSpaceInput input)
        {
            try
            {
                return await ProcessAsync(input);
            }
            catch (Exception exception) when (exception.GetBaseException() is PostgresException ex)
            {
                logger.LogError(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                throw new ValidationException($"An error occurred that prevented creation of the \"{input.Name}\" space. {ex.MessageText.TrimEnd('.')}. {ex.Detail}", ex);
            }
        }

        private async Task<CreateSpaceResult> ProcessAsync(CreateSpaceInput input)
        {
            validationService.Validate(input);

            var elevatedDataService = userDataServiceFactory.NewElevatedDataService(input.DatabaseName);
            using var elevatedDbContext = userDbContextFactory.NewElevatedDbContext<EtcDbContext>(input.DatabaseName);
            await using var tx = await elevatedDbContext.Database.BeginTransactionAsync();

            var space = new Node()
            {
                NodeId = Guid.NewGuid(),
                ParentId = NodeId.Root,
                Name = input.Name,
                TypeId = "Space",
                Created = DateTime.UtcNow,
            };

            validationService.Validate(space);
            elevatedDbContext.Nodes.Add(space);
            await elevatedDbContext.SaveChangesAsync();

            string schemaName = input.Name;

            // Create the SCHEMA as the pg_database_owner.
            await elevatedDataService.ExecuteAsync(
                $"SET ROLE pg_database_owner",
                $"CREATE SCHEMA {Sql.Identifier(schemaName)}");

            // Commit the new Space record
            await tx.CommitAsync();

            return new()
            {
                NodeId = space.NodeId,
            };
        }
    }
}
