using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
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
    private readonly ValidationService validationService;
    private readonly UserDataServiceFactory userDataServiceFactory;
    private readonly GrantSpaceService grantSpaceService;

    public CreateSpaceService(
        ValidationService validationService,
        UserDataServiceFactory userDataServiceFactory,
        GrantSpaceService grantSpaceService)
    {
        this.validationService = validationService;
        this.userDataServiceFactory = userDataServiceFactory;
        this.grantSpaceService = grantSpaceService;
    }

    public async Task<Inode> CreateSpaceAsync(CreateSpaceInput input)
    {
        validationService.Validate(input);

        var space = new Inode()
        {
            InodeId = Guid.NewGuid(),
            ParentInodeId = InodeId.Root,
            Name = input.SpaceName,
            UglyName = TextTransformers.Snakify(input.SpaceName),
            InodeTypeId = InodeTypeId.Space,
        };

        validationService.Validate(space);

        string schemaName = space.UglyName;

        // Create the SCHEMA as the pg_database_owner.
        var schemaCreated = false;
        var elevatedDataService = userDataServiceFactory.NewElevatedDataService(input.OrganizationId);
        try
        {
            await elevatedDataService.ExecuteAsync(
                Sql.Format($"SET ROLE pg_database_owner"),
                Sql.Format($"CREATE SCHEMA {Sql.Identifier(schemaName)}"),
                Sql.Insert(space));

            schemaCreated = true;

            await grantSpaceService.GrantSpaceAsync(
                input.OrganizationId,
                space.InodeId,
                schemaName,
                input.AccessControlList);
        }
        catch (Exception)
        {
            if (schemaCreated)
            {
                await elevatedDataService.ExecuteAsync(
                    Sql.Format($"SET ROLE pg_database_owner"),
                    Sql.Delete(space),
                    Sql.Format($"DROP SCHEMA IF EXISTS {Sql.Identifier(schemaName)}"));
            }

            throw;
        }

        return space;
    }
}
