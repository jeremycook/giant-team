using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organization.Etc.Data;
using GiantTeam.Organization.Etc.Models;
using GiantTeam.Postgres;
using GiantTeam.Text;
using GiantTeam.UserData.Services;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Services;

public class CreateSpaceInput
{
    [RequiredGuid]
    public Guid OrganizationId { get; set; }

    [Required, StringLength(50), InodeName]
    public string SpaceName { get; set; } = null!;

    [Required]
    public InodeAccess[] AccessControlList { get; set; } = null!;
}

public class CreateSpaceService
{
    private readonly ILogger<CreateSpaceService> logger;
    private readonly ValidationService validationService;
    private readonly UserDataServiceFactory userDataServiceFactory;
    private readonly GrantSpaceService grantSpaceService;
    private readonly FetchInodeService fetchInodeService;

    public CreateSpaceService(
        ILogger<CreateSpaceService> logger,
        ValidationService validationService,
        UserDataServiceFactory userDataServiceFactory,
        GrantSpaceService grantSpaceService,
        FetchInodeService fetchInodeService)
    {
        this.logger = logger;
        this.validationService = validationService;
        this.userDataServiceFactory = userDataServiceFactory;
        this.grantSpaceService = grantSpaceService;
        this.fetchInodeService = fetchInodeService;
    }

    public async Task<Inode> CreateSpaceAsync(CreateSpaceInput input)
    {
        validationService.Validate(input);

        var space = new InodeRecord(DateTime.UtcNow)
        {
            InodeId = Guid.NewGuid(),
            ParentInodeId = InodeId.Root,
            Name = input.SpaceName,
            UglyName = TextTransformers.Snakify(input.SpaceName),
            InodeTypeId = InodeTypeId.Space,
        };

        validationService.Validate(space);

        string schemaName = space.UglyName;

        var elevatedDataService = userDataServiceFactory.NewElevatedDataService(input.OrganizationId);

        // Create the SCHEMA as the pg_database_owner.
        await elevatedDataService.ExecuteAsync(
            Sql.Format($"SET ROLE pg_database_owner"),
            Sql.Format($"CREATE SCHEMA {Sql.Identifier(schemaName)}"),
            Sql.Insert(space));

        try
        {
            // Grant access
            await grantSpaceService.GrantSpaceAsync(
                input.OrganizationId,
                space.InodeId,
                schemaName,
                input.AccessControlList);
        }
        catch (Exception)
        {
            try
            {
                await elevatedDataService.ExecuteAsync(
                    Sql.Format($"SET ROLE pg_database_owner"),
                    Sql.Delete(space),
                    Sql.Format($"DROP SCHEMA IF EXISTS {Sql.Identifier(schemaName)}"));
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Error unwinding create {Schema} in {OrganizationId}.", schemaName, input.OrganizationId);
            }

            throw;
        }

        var result = await fetchInodeService.FetchInodeAsync(input.OrganizationId, space.InodeId);
        return result;
    }
}
