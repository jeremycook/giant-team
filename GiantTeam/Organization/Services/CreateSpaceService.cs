using GiantTeam.Cluster.Directory.Helpers;
using GiantTeam.Cluster.Directory.Services;
using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Linq;
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
        public string OrganizationId { get; set; } = null!;

        [Required, StringLength(50), InodeName]
        public string SpaceName { get; set; } = null!;

        [Required, MinLength(1)]
        public List<GrantSpaceInputGrant> Grants { get; set; } = null!;
    }

    public class CreateSpaceResult
    {
        public Guid InodeId { get; set; }
    }

    public class CreateSpaceService
    {
        private readonly ILogger<CreateSpaceService> logger;
        private readonly ValidationService validationService;
        private readonly FetchOrganizationService fetchOrganizationService;
        private readonly UserDbContextFactory userDbContextFactory;
        private readonly UserDataServiceFactory userDataServiceFactory;

        public CreateSpaceService(
            ILogger<CreateSpaceService> logger,
            ValidationService validationService,
            FetchOrganizationService fetchOrganizationService,
            UserDbContextFactory userDbContextFactory,
            UserDataServiceFactory userDataServiceFactory)
        {
            this.logger = logger;
            this.validationService = validationService;
            this.fetchOrganizationService = fetchOrganizationService;
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
                throw new ValidationException($"An error occurred that prevented creation of the \"{input.SpaceName}\" space. {ex.MessageText.TrimEnd('.')}. {ex.Detail}", ex);
            }
        }

        private async Task<CreateSpaceResult> ProcessAsync(CreateSpaceInput input)
        {
            validationService.Validate(input);

            var space = new Etc.Data.Inode()
            {
                InodeId = Guid.NewGuid(),
                ParentInodeId = InodeId.Root,
                Name = input.SpaceName,
                InodeTypeId = "Space",
                Created = DateTime.UtcNow,
            };

            validationService.Validate(space);

            await using var elevatedDbContext = userDbContextFactory.NewElevatedDbContext<EtcDbContext>(input.OrganizationId);
            await using var tx = await elevatedDbContext.Database.BeginTransactionAsync();
            elevatedDbContext.Inodes.Add(space);
            await elevatedDbContext.SaveChangesAsync();

            string schemaName = input.SpaceName;

            // Create the SCHEMA as the pg_database_owner.
            var commands = new List<Sql>()
            {
                Sql.Format($"SET ROLE pg_database_owner"),
                Sql.Format($"CREATE SCHEMA {Sql.Identifier(schemaName)}"),
            };
            commands.AddRange(input.Grants.SelectMany(p => new[]
            {
                Sql.Format($"GRANT {(p.Privileges.Contains(GrantSpaceInputPrivilege.ALL) ?
                    Sql.Raw(GrantSpaceInputPrivilege.ALL.ToString()) :
                    Sql.Raw(p.Privileges.Select(o => o.ToString()).Join(","))
                )} ON SCHEMA {Sql.Identifier(schemaName)} TO {Sql.Identifier(DirectoryHelpers.OrganizationRole(p.OrganizationRoleId)!)}"),
            }));

            var elevatedDataService = userDataServiceFactory.NewElevatedDataService(input.OrganizationId);
            await elevatedDataService.ExecuteAsync(commands);

            // Commit the new Space record
            await tx.CommitAsync();

            return new()
            {
                InodeId = space.InodeId,
            };
        }
    }
}
