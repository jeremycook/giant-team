namespace GiantTeam.Organization.Etc.Models;

public class OrganizationDetails
{
    public Guid OrganizationId { get; init; }

    public IReadOnlyDictionary<string, InodeType> InodeTypes { get; init; } = null!;

    public IEnumerable<Role> Roles { get; init; } = null!;

    public Inode RootInode { get; init; } = null!;
    public IEnumerable<Inode> RootChildren { get; init; } = null!;
}
