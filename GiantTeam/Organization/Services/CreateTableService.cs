using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organization.Etc.Data;
using GiantTeam.Organization.Etc.Models;
using GiantTeam.Postgres;
using GiantTeam.UserData.Services;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GiantTeam.Organization.Services;

public class CreateTableInput
{
    [RequiredGuid]
    public Guid OrganizationId { get; set; }

    [RequiredGuid]
    public Guid ParentInodeId { get; set; }

    [Required, StringLength(50), InodeName]
    public string TableName { get; set; } = null!;

    [Required]
    public List<InodeAccess> AccessControlList { get; set; } = null!;
}

public class CreateTableService
{
    private readonly ILogger<CreateTableService> logger;
    private readonly ValidationService validationService;
    private readonly InodeTypeService inodeTypeService;
    private readonly FetchInodeService fetchInodeService;
    private readonly UserDataServiceFactory userDataServiceFactory;
    private readonly GrantTableService grantTableService;

    public CreateTableService(
        ILogger<CreateTableService> logger,
        ValidationService validationService,
        InodeTypeService inodeTypeService,
        FetchInodeService fetchInodeService,
        UserDataServiceFactory userDataServiceFactory,
        GrantTableService grantTableService)
    {
        this.logger = logger;
        this.validationService = validationService;
        this.inodeTypeService = inodeTypeService;
        this.fetchInodeService = fetchInodeService;
        this.userDataServiceFactory = userDataServiceFactory;
        this.grantTableService = grantTableService;
    }

    /// <summary>
    /// Returns the <see cref="Inode"/> of the new table.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<Inode> CreateTableAsync(CreateTableInput input)
    {
        validationService.Validate(input);

        var parentInode = await fetchInodeService.FetchInodeAsync(input.OrganizationId, input.ParentInodeId);

        if (!await inodeTypeService.CanContainAsync(input.OrganizationId, parentInode.InodeTypeId, InodeTypeId.Table))
        {
            throw new InvalidRequestException($"A Table cannot be created in a {parentInode.InodeTypeId}.");
        }

        var tableRecord = new InodeRecord(DateTime.UtcNow)
        {
            InodeId = Guid.NewGuid(),
            ParentInodeId = input.ParentInodeId,
            Name = input.TableName,
            InodeTypeId = InodeTypeId.Table,
        };

        validationService.Validate(tableRecord);

        var space = await fetchInodeService.FetchInodeByPathAsync(input.OrganizationId, parentInode.Path.Split('/').First());
        if (space.InodeTypeId != InodeTypeId.Space)
        {
            throw new InvalidOperationException($"Expected a Space but found a {space.InodeTypeId}.");
        }

        string schemaName = space.UglyName;
        string tableName = tableRecord.UglyName;

        var createAndGrantCommands = new[]
        {
            Sql.Format($"SET ROLE pg_database_owner"),
            Sql.Format($"CREATE TABLE {Sql.Identifier(schemaName, tableName)} ()"),
            Sql.Insert(tableRecord),
        }.Concat(grantTableService.GenerateGrantTableCommands(schemaName, tableName, tableRecord.InodeId, input.AccessControlList));

        var elevatedDataService = userDataServiceFactory.NewElevatedDataService(input.OrganizationId);
        await elevatedDataService.ExecuteAsync(createAndGrantCommands);

        var result = await fetchInodeService.FetchInodeAsync(input.OrganizationId, tableRecord.InodeId);
        return result;
    }
}
