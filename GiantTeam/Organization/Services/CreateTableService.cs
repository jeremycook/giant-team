using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organization.Etc.Models;
using GiantTeam.Postgres;
using GiantTeam.UserData.Services;
using System.ComponentModel.DataAnnotations;

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
    private readonly ValidationService validationService;
    private readonly InodeTypeService inodeTypeService;
    private readonly FetchInodeService fetchInodeService;
    private readonly UserDataServiceFactory userDataServiceFactory;
    private readonly GrantTableService grantTableService;

    public CreateTableService(
        ValidationService validationService,
        InodeTypeService inodeTypeService,
        FetchInodeService fetchInodeService,
        UserDataServiceFactory userDataServiceFactory,
        GrantTableService grantTableService)
    {
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

        var table = new Inode()
        {
            InodeId = Guid.NewGuid(),
            ParentInodeId = input.ParentInodeId,
            Name = input.TableName,
            InodeTypeId = InodeTypeId.Table,
        };

        validationService.Validate(table);

        var space = await fetchInodeService.FetchInodeByPathAsync(input.OrganizationId, parentInode.Path.Split('/').First());
        if (space.InodeTypeId != InodeTypeId.Space)
        {
            throw new InvalidOperationException($"Expected a Space but found a {space.InodeTypeId}.");
        }

        string schemaName = space.UglyName;
        string tableName = table.UglyName;

        var elevatedDataService = userDataServiceFactory.NewElevatedDataService(input.OrganizationId);
        await elevatedDataService.ExecuteAsync(
            Sql.Format($"CREATE TABLE {Sql.Identifier(schemaName, tableName)}"),
            Sql.Insert(table));
        await grantTableService.GrantTableAsync(
            input.OrganizationId,
            space,
            table,
            input.AccessControlList);

        return await fetchInodeService.FetchInodeAsync(input.OrganizationId, table.InodeId);
    }
}
