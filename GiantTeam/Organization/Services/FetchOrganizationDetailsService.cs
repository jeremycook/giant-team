using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.UserData.Services;
using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Organization.Services;

public class FetchOrganizationDetailsService
{
    private readonly ValidationService validationService;
    private readonly UserDirectoryDbContextFactory userDirectoryDbContextFactory;

    public FetchOrganizationDetailsService(
        ValidationService validationService,
        UserDirectoryDbContextFactory userDirectoryDbContextFactory)
    {
        this.validationService = validationService;
        this.userDirectoryDbContextFactory = userDirectoryDbContextFactory;
    }

    public async Task<FetchOrganizationDetailsResult> FetchOrganizationDetailsAsync(FetchOrganizationDetailsInput input)
    {
        validationService.Validate(input);

        using var directoryDb = userDirectoryDbContextFactory.NewDbContext();

        var org = await directoryDb.Organizations
            .Include(o => o.Roles)
            .SingleOrDefaultAsync(o => o.OrganizationId == input.OrganizationId) ??
            throw new NotFoundException($"The \"{input.OrganizationId}\" organization was not found.");

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
        };
        return result;
    }
}

public class FetchOrganizationDetailsInput
{
    public string OrganizationId { get; set; } = null!;
}

public class FetchOrganizationDetailsResult
{
    public string OrganizationId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public DateTime Created { get; set; }
    public FetchOrganizationDetailsRole[] Roles { get; set; } = null!;
}

public class FetchOrganizationDetailsRole
{
    public string Name { get; set; } = null!;
    public string DbRole { get; set; } = null!;
}