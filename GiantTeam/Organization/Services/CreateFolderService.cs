using GiantTeam.Cluster.Directory.Services;
using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organization.Etc.Data;
using GiantTeam.Organization.Etc.Models;
using GiantTeam.UserData.Services;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Services;

public class CreateFolderInput
{
    [RequiredGuid]
    public Guid OrganizationId { get; set; }

    [RequiredGuid]
    public Guid ParentInodeId { get; set; }

    [Required, StringLength(50), InodeName]
    public string FolderName { get; set; } = null!;

    //[Required, MinLength(1)]
    //public List<GrantFolderInputGrant> Grants { get; set; } = null!;
}

public class CreateFolderResult
{
    public Guid InodeId { get; set; }
}

public class CreateFolderService
{
    private readonly ILogger<CreateFolderService> logger;
    private readonly ValidationService validationService;
    private readonly FetchOrganizationService fetchOrganizationService;
    private readonly UserDbContextFactory userDbContextFactory;
    private readonly UserDataServiceFactory userDataServiceFactory;

    public CreateFolderService(
        ILogger<CreateFolderService> logger,
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

    public async Task<CreateFolderResult> CreateFolderAsync(CreateFolderInput input)
    {
        try
        {
            return await ProcessAsync(input);
        }
        catch (Exception exception) when (exception.GetBaseException() is PostgresException ex)
        {
            logger.LogError(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
            throw new ValidationException($"An error occurred that prevented creation of the \"{input.FolderName}\" folder. {ex.MessageText.TrimEnd('.')}. {ex.Detail}", ex);
        }
    }

    private async Task<CreateFolderResult> ProcessAsync(CreateFolderInput input)
    {
        validationService.Validate(input);

        var folder = new Etc.Data.Inode()
        {
            InodeId = Guid.NewGuid(),
            ParentInodeId = input.ParentInodeId,
            Name = input.FolderName,
            InodeTypeId = InodeTypeId.Folder,
            Created = DateTime.UtcNow,
        };

        validationService.Validate(folder);

        await using var elevatedDbContext = userDbContextFactory.NewElevatedDbContext<EtcDbContext>(input.OrganizationId);
        elevatedDbContext.Inodes.Add(folder);
        await elevatedDbContext.SaveChangesAsync();

        return new()
        {
            InodeId = folder.InodeId,
        };
    }
}
