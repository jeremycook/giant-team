using GiantTeam.Cluster.Directory.Services;
using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organization.Etc.Models;
using GiantTeam.Postgres;
using GiantTeam.UserData.Services;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Services;

public class CreateSpaceInput
{
    [Required, StringLength(50)]
    public string OrganizationId { get; set; } = null!;

    [Required, StringLength(50), InodeName]
    public string SpaceName { get; set; } = null!;

    [Required]
    public Etc.Models.InodeAccess[] AccessControlList { get; set; } = null!;
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
    private readonly GrantSpaceService grantSpaceService;

    public CreateSpaceService(
        ILogger<CreateSpaceService> logger,
        ValidationService validationService,
        FetchOrganizationService fetchOrganizationService,
        UserDbContextFactory userDbContextFactory,
        UserDataServiceFactory userDataServiceFactory,
        GrantSpaceService grantSpaceService)
    {
        this.logger = logger;
        this.validationService = validationService;
        this.fetchOrganizationService = fetchOrganizationService;
        this.userDbContextFactory = userDbContextFactory;
        this.userDataServiceFactory = userDataServiceFactory;
        this.grantSpaceService = grantSpaceService;
    }

    public async Task<CreateSpaceResult> CreateSpaceAsync(CreateSpaceInput input)
    {
        return await ProcessAsync(input);
    }

    private async Task<CreateSpaceResult> ProcessAsync(CreateSpaceInput input)
    {
        validationService.Validate(input);

        var space = new Etc.Data.Inode()
        {
            InodeId = Guid.NewGuid(),
            ParentInodeId = InodeId.Root,
            Name = input.SpaceName,
            InodeTypeId = InodeTypeId.Space,
            Created = DateTime.UtcNow,
        };

        validationService.Validate(space);

        string schemaName = space.UglyName;

        // Create the SCHEMA as the pg_database_owner.
        var elevatedDataService = userDataServiceFactory.NewElevatedDataService(input.OrganizationId);
        try
        {
            await elevatedDataService.ExecuteAsync(
                Sql.Format($"SET ROLE pg_database_owner"),
                Sql.Format($"CREATE SCHEMA {Sql.Identifier(schemaName)}"),
                Sql.Insert(space));

            await grantSpaceService.GrantSpaceAsync(
                input.OrganizationId,
                space.InodeId,
                schemaName,
                input.AccessControlList);
        }
        catch (Exception)
        {
            await elevatedDataService.ExecuteAsync(
                Sql.Format($"SET ROLE pg_database_owner"),
                Sql.Format($"DROP SCHEMA IF EXISTS {Sql.Identifier(schemaName)}"));

            throw;
        }

        return new()
        {
            InodeId = space.InodeId,
        };
    }
}
