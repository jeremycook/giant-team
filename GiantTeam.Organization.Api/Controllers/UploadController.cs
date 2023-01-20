using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organization.Etc.Data;
using GiantTeam.Organization.Etc.Models;
using GiantTeam.Organization.Services;
using GiantTeam.UserData.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Api.Controllers;

public class UploadController : ControllerBase
{
    [HttpPost("/api/organization/[Controller]")]
    public async Task<UploadResult> Post(
        [FromServices] ValidationService validationService,
        [FromServices] FetchInodeService fetchInodeService,
        [FromServices] InodeTypeService inodeTypeService,
        [FromServices] UserDbContextFactory userDbContextFactory,
        [FromForm] UploadInput input)
    {
        var inode = await fetchInodeService.FetchInodeByPathAsync(new()
        {
            OrganizationId = input.OrganizationId,
            Path = input.Path,
        });

        if (!await inodeTypeService.CanContainAsync(input.OrganizationId, inode.InodeTypeId, InodeTypeId.File))
        {
            throw new InvalidOperationException($"A File cannot be uploaded into the {inode.InodeTypeId} at {inode.Path}.");
        }

        var newInodes = new List<InodeRecord>();

        await using var db = userDbContextFactory.NewDbContext<EtcDbContext>(input.OrganizationId);
        foreach (var upload in input.Files)
        {
            if (upload.Length <= 0)
            {
                continue;
            }

            InodeRecord fileInode = new(DateTime.UtcNow)
            {
                InodeId = Guid.NewGuid(),
                ParentInodeId = inode.InodeId,
                InodeTypeId = InodeTypeId.File,
                Name = Path.GetFileName(upload.FileName),
            };

            FileRecord file;
            {
                await using var stream = upload.OpenReadStream();
                using var memory = new MemoryStream();
                file = new()
                {
                    InodeId = fileInode.InodeId,
                    ContentType = upload.ContentType,
                    Data = memory.ToArray(),
                };
            }

            validationService.ValidateAll(fileInode, file);

            db.AddRange(fileInode, file);
            newInodes.Add(fileInode);
        }
        await db.SaveChangesAsync(); // All or none

        return new UploadResult
        {
            UploadedInodes = newInodes.Select(Inode.CreateFrom).ToArray(),
        };
    }
}

public class UploadInput
{
    [RequiredGuid]
    public Guid OrganizationId { get; set; }

    // TODO: Change Path to InodeId
    [Required]
    public string Path { get; set; } = null!;

    [Required, MinLength(1)]
    public IFormFileCollection Files { get; set; } = null!;
}

public class UploadResult
{
    public IEnumerable<Inode> UploadedInodes { get; set; } = null!;
}
