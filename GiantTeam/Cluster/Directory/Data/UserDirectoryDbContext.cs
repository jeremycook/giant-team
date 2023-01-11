using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Cluster.Directory.Data;

public class UserDirectoryDbContext : DirectoryDbContext<UserDirectoryDbContext>
{
    public UserDirectoryDbContext(DbContextOptions<UserDirectoryDbContext> options)
        : base(options) { }
}
