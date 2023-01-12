using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organization.Etc.Data;
using GiantTeam.Organization.Etc.Models;
using GiantTeam.UserData.Services;
using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Organization.Services;

public class FetchOrganizationDetailsService
{
    private readonly ValidationService validationService;
    private readonly UserDirectoryDbContextFactory userDirectoryDbContextFactory;
    private readonly UserDbContextFactory userDbContextFactory;

    public FetchOrganizationDetailsService(
        ValidationService validationService,
        UserDirectoryDbContextFactory userDirectoryDbContextFactory,
        UserDbContextFactory userDbContextFactory)
    {
        this.validationService = validationService;
        this.userDirectoryDbContextFactory = userDirectoryDbContextFactory;
        this.userDbContextFactory = userDbContextFactory;
    }

    public async Task<FetchOrganizationDetailsResult> FetchOrganizationDetailsAsync(FetchOrganizationDetailsInput input)
    {
        validationService.Validate(input);

        using var directoryDb = userDirectoryDbContextFactory.NewDbContext();

        var org = await directoryDb.Organizations
            .Include(o => o.Roles)
            .SingleOrDefaultAsync(o => o.OrganizationId == input.OrganizationId) ??
            throw new NotFoundException($"The \"{input.OrganizationId}\" organization was not found.");

        using var etcDb = userDbContextFactory.NewDbContext<EtcDbContext>(org.DatabaseName, "etc");
        var spaces = await etcDb.Nodes
            .Where(o => o.ParentId == NodeId.Root && o.NodeId != o.ParentId)
            .OrderBy(o => o.Name)
            .ToListAsync();

        var result = new FetchOrganizationDetailsResult()
        {
            OrganizationId = org.OrganizationId,
            Name = org.Name,
            DatabaseName = org.DatabaseName,
            Created = org.Created,
            Roles = org.Roles!.Select(r => new FetchOrganizationDetailsRole()
            {
                Name = r.Name,
                DbRole = r.DbRole,
            }).ToArray(),
            Spaces = spaces.Select(n => new FetchOrganizationDetailsSpace()
            {
                NodeId = n.NodeId,
                ParentId = n.ParentId,
                TypeId = n.TypeId,
                Name = n.Name,
                Created = n.Created,
            }).ToArray(),
        };
        return result;
    }
}

public class FetchOrganizationDetailsSpace
{
    public Guid NodeId { get; init; }
    public Guid ParentId { get; init; }
    public string TypeId { get; init; } = null!;
    public string Name { get; init; } = null!;
    public DateTime Created { get; init; }
}

public class FetchOrganizationDetailsInput
{
    public string OrganizationId { get; set; } = null!;
}

public class FetchOrganizationDetailsResult
{
    public string OrganizationId { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string DatabaseName { get; init; } = null!;
    public DateTime Created { get; init; }
    public FetchOrganizationDetailsRole[] Roles { get; init; } = null!;
    public FetchOrganizationDetailsSpace[] Spaces { get; init; } = null!;
}

public class FetchOrganizationDetailsRole
{
    public string Name { get; init; } = null!;
    public string DbRole { get; init; } = null!;
}