using GiantTeam.ComponentModel.Services;
using GiantTeam.Organization.Etc.Data;
using GiantTeam.Organization.Etc.Models;
using GiantTeam.Organization.Services;
using GiantTeam.UserData.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Api.Controllers;

public class UploadController : ControllerBase
{
    [HttpPost("/api/organization/[Controller]")]
    public async Task<UploadResult> Post(
        [FromServices] ValidationService validationService,
        [FromServices] FetchInodeService fetchInodeService,
        [FromServices] UserDbContextFactory userDbContextFactory,
        [FromForm] UploadInput input)
    {
        var inode = await fetchInodeService.FetchInodeByPathAsync(new()
        {
            OrganizationId = input.OrganizationId,
            Path = input.Path,
        });

        if (!inode.ChildrenConstraints.Any(c => c.InodeTypeId == InodeTypeId.File))
        {
            throw new InvalidOperationException($"Files cannot be uploaded into the {inode.InodeTypeId} at {inode.Path}.");
        }

        var newInodes = new List<Etc.Data.Inode>();

        await using var db = userDbContextFactory.NewDbContext<EtcDbContext>(input.OrganizationId);
        await using var tx = await db.Database.BeginTransactionAsync();
        foreach (var upload in input.Files)
        {
            if (upload.Length <= 0)
            {
                continue;
            }

            Etc.Data.Inode fileInode = new()
            {
                InodeId = Guid.NewGuid(),
                ParentInodeId = inode.InodeId,
                InodeTypeId = InodeTypeId.File,
                Name = Path.GetFileName(upload.FileName),
                Created = DateTime.UtcNow,
            };

            Etc.Data.File file;
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
            await db.SaveChangesAsync();
        }
        await tx.CommitAsync(); // All or none

        var fileInodeTypeConstraints = await (
            from con in db.InodeTypeConstraints
            where con.ParentInodeTypeId == InodeTypeId.File && con.InodeTypeId != con.ParentInodeTypeId
            orderby con.InodeTypeId
            select new InodeChildConstraint()
            {
                InodeTypeId = con.InodeTypeId,
            }
        ).ToListAsync();

        return new UploadResult
        {
            UploadedInodes = newInodes
                .Select(o => new Etc.Models.Inode()
                {
                    InodeTypeId = o.InodeTypeId,
                    InodeId = o.InodeId,
                    ParentInodeId = o.ParentInodeId,
                    Name = o.Name,
                    Created = o.Created,
                    Path = o.Path,
                    ChildrenConstraints = fileInodeTypeConstraints,
                    Children = null,
                })
            .ToArray(),
        };
    }
}

public class UploadInput
{
    [Required]
    public string OrganizationId { get; set; } = null!;

    // TODO: Change Path to InodeId
    [Required]
    public string Path { get; set; } = null!;

    [Required, MinLength(1)]
    public IFormFileCollection Files { get; set; } = null!;
}

public class UploadResult
{
    public Etc.Models.Inode[] UploadedInodes { get; set; } = null!;
}
